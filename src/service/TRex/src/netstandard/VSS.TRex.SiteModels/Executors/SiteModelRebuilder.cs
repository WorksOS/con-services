using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Serilog.Extensions;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.Executors;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Utilities;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;

namespace VSS.TRex.SiteModels
{
  /// <summary>
  /// Defines and manages the activites executed asynchronously to rebuild a project. This class is instantiated
  /// by the project rebuilder manager for each project sbeing rebuilt
  /// </summary>
  public class SiteModelRebuilder : ISiteModelRebuilder
  {
    private static ILogger _log = Logging.Logger.CreateLogger<SiteModelRebuilder>();

    /// <summary>
    /// Length of time to wait between monitoring epochs
    /// </summary>
    private const int kMonitoringDelayMS = 10000;

    /// <summary>
    /// The key name to be used for the metadata entries in the metadata cache (one per project)
    /// </summary>
    private static string kMetadataKeyName = "metadata";

    /// <summary>
    /// The storage proxy cache for the rebuilder to use for tracking metadata
    /// </summary>
    public IStorageProxyCache<INonSpatialAffinityKey, IRebuildSiteModelMetaData> MetadataCache { get; set; }

    /// <summary>
    /// The storage proxy cache for the rebuilder to use to store names of TAG files requested from S3
    /// </summary>
    public IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> FilesCache { get; set; }

    /// <summary>
    /// Project ID of this project this rebuilder is managing
    /// </summary>
    public Guid ProjectUid { get; }

    private IRebuildSiteModelMetaData _metadata;
    public IRebuildSiteModelMetaData Metadata => _metadata;

    private bool _aborted = false;

    private CancellationTokenSource _cancellationSource = new CancellationTokenSource();

    public SiteModelRebuilder(Guid projectUid, bool archiveTAGFiles)
    {
      ProjectUid = projectUid;

      RebuildSiteModelFlags flags = archiveTAGFiles ? RebuildSiteModelFlags.AddProcessedTagFileToArchive : 0;

      _metadata = new RebuildSiteModelMetaData()
      {
        ProjectUID = projectUid,
        Flags = flags
      };

     // _response = new RebuildSiteModelRequestResponse(projectUid);
    }

    /// <summary>
    /// Aborts the current project rebuild operation in it's current state
    /// </summary>
    public void Abort()
    {
      _aborted = true;
      _cancellationSource.Cancel();
    }

    /// <summary>
    /// Reads the rebuld site model metadata for the given project from the persistent cache
    /// </summary>
    private IRebuildSiteModelMetaData GetMetaData(Guid projectUid)
    {
      try
      {
        return MetadataCache.Get(new NonSpatialAffinityKey(projectUid, kMetadataKeyName));
      }
      catch (KeyNotFoundException)
      {
        // No metadata present - just return null
        return null;
      }
    }

    /// <summary>
    /// Updates the state of the rebuild site meta data for the project in the persistent store
    /// </summary>
    private void UpdateMetaData()
    {
      _metadata.LastUpdateUtcTicks = DateTime.UtcNow.Ticks;
      MetadataCache.Put(new NonSpatialAffinityKey(ProjectUid, kMetadataKeyName), _metadata);
    }

    /// <summary>
    /// Moves the metadata to the next phases in the process
    /// </summary>
    private void UpdatePhase(RebuildSiteModelPhase phase)
    {
      _metadata.Phase = phase;
      UpdateMetaData();
    }

    /// <summary>
    /// Defines the phase state transitions
    /// </summary>
    private RebuildSiteModelPhase NextPhase(RebuildSiteModelPhase phase)
    {
      return phase switch
      {
        RebuildSiteModelPhase.Unknown => RebuildSiteModelPhase.Deleting,
        RebuildSiteModelPhase.Deleting => RebuildSiteModelPhase.Scanning,
        RebuildSiteModelPhase.Scanning => RebuildSiteModelPhase.Submitting,
        RebuildSiteModelPhase.Submitting => RebuildSiteModelPhase.Monitoring,
        RebuildSiteModelPhase.Monitoring => RebuildSiteModelPhase.Completion,
        RebuildSiteModelPhase.Completion => RebuildSiteModelPhase.Complete,
        RebuildSiteModelPhase.Complete => RebuildSiteModelPhase.Unknown,
        _ => throw new TRexException($"Unknown rebuild site model phase {phase}")
      };
    }

    /// <summary>
    /// Moves to the next processing phase given the currnt phase
    /// </summary>
    private void AdvancePhase(ref RebuildSiteModelPhase currentPhase)
    {
      UpdatePhase(NextPhase(currentPhase));
    }

    /// <summary>
    /// Ensures there is no current rebuilding activity for this projecy.
    /// The exception is when there exists a metadata record for the project and the phase is complete
    /// </summary>
    public bool ValidateNoAciveRebuilderForProject(Guid projectUid)
    {
      var metadata = GetMetaData(projectUid);

      return metadata == null || metadata.Phase == RebuildSiteModelPhase.Complete;
    }

    /// <summary>
    /// Performs a partial delection of the site model ready for data to be reprocessed into it.
    /// </summary>
    private async Task<bool> ExecuteProjectDelete()
    {
      var deleteRequest = new DeleteSiteModelRequest();

      var result = await deleteRequest.ExecuteAsync(new DeleteSiteModelRequestArgument
      {
        ProjectID = ProjectUid,
        Selectivity = _metadata.DeletionSelectivity
      });

      _metadata.DeletionResult = result.Result;

      if (result.Result != DeleteSiteModelResult.OK)
        _metadata.RebuildResult = RebuildSiteModelResult.FailedToDeleteSiteModel;

      UpdateMetaData();

      return result.Result == DeleteSiteModelResult.OK;
    }

    /// <summary>
    /// Scans the source S3 lcoation for all tag files to be submitted and places these names into a cache to 
    /// be used as the source for the TAG files submisstion phase.
    /// </summary>
    private async Task ExecuteTAGFileScanning()
    {
      var s3FileTransfer = new S3FileTransfer(_metadata.OriginS3TransferProxy);
      var continuation = string.Empty;
      var runningCount = 0;
      do
      {
        var (candidateTAGFiles, nextContinuation) = await s3FileTransfer.ListKeys($"/{_metadata.ProjectUID}", 1000, continuation);
        continuation = nextContinuation;
        runningCount += candidateTAGFiles.Length;

        // Put the candidate TAG files into the cache
        var sb = new StringBuilder(2 * (candidateTAGFiles.Sum(x => x.Length) + 1));
        sb.AppendJoin('|', candidateTAGFiles);

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        using (var compressedStream = MemoryStreamCompression.Compress(ms))
        {
          if (_log.IsTraceEnabled())
            _log.LogInformation($"Putting block of {candidateTAGFiles.Length} TAG file names for project {ProjectUid}");
          FilesCache.Put(new NonSpatialAffinityKey(ProjectUid, runningCount.ToString(CultureInfo.InvariantCulture)), new SerialisedByteArrayWrapper(compressedStream.ToArray()));
        }
      } while (!string.IsNullOrEmpty(continuation));

      _metadata.NumberOfTAGFilesFromS3 = runningCount;
    }

    /// <summary>
    /// Iterates over all elements stored in the file cache and requests those files from 3, submitting them
    /// to the TAG file processor as it goes.
    /// </summary>
    private async Task<bool> ExecuteTAGFileSubmission()
    {
      var tagFileCollection = new List<string>();

      // Read all collections into a single list
      for (var i = 0; i < _metadata.NumberOfTAGFileKeyCollections; i++)
      {
        var key = new NonSpatialAffinityKey(ProjectUid, i.ToString(CultureInfo.InvariantCulture));
        try
        {
          using var ms = new MemoryStream((await FilesCache.GetAsync(key)).Bytes);
          var uncompressedTagFileCollection = MemoryStreamCompression.Decompress(ms);
          tagFileCollection.AddRange(Encoding.UTF8.GetString(uncompressedTagFileCollection.ToArray()).Split('|'));
        }
        catch (KeyNotFoundException e)
        {
          _log.LogError(e, $"Key {key} not found. Aborting.");
          _metadata.RebuildResult = RebuildSiteModelResult.UnableToLocateTAGFileKeyCollection;
          UpdateMetaData();
          return false;
        }
      }

      // Sort the list based on time contained in the name of the tag file
      tagFileCollection.OrderBy(x => Convert.ToUInt64(x.Split('/')[1].Split("--")[2]));

      var s3FileTransfer = new S3FileTransfer(_metadata.OriginS3TransferProxy);

      var foundPointToStartSubmittingTAGFiles = string.IsNullOrEmpty(_metadata.LastSubmittedTagFile);

      // Iterate across the sorted collection submitting each one in turn to the TAG file processor. 
      // After successful submission update metadata with name of submitted file
      foreach (var tagFileKey in tagFileCollection)
      {
        // Make some determinaton that the key looks valid and defines a *.tag file
        // TODO - complete this check


        // If the last submitted tag file is not null in the metadata until that key is encountered before
        // submitting more TAG files
        if (!foundPointToStartSubmittingTAGFiles)
        {
          foundPointToStartSubmittingTAGFiles = _metadata.LastSubmittedTagFile.Equals(tagFileKey, StringComparison.InvariantCultureIgnoreCase);

          if (!foundPointToStartSubmittingTAGFiles)
            continue;
        }

        // Determine the asset ID from the key of form '<{MachineUid}>/<TagFileName>'
        var split = tagFileKey.Split('/');
        var assetID = Guid.Parse(split[0]);
        var tagFileName = split[1];

        // Read the content of the TAG file from S3
        var tagFile = await s3FileTransfer.Proxy.Download(tagFileKey);
        using var ms = new MemoryStream();
        await tagFile.FileStream.CopyToAsync(ms);

        // Submit the file...
        var submissionRequest = new SubmitTAGFileRequest();
        var submissionResult = await submissionRequest.ExecuteAsync(new SubmitTAGFileRequestArgument
        {
          ProjectID = ProjectUid,
          AssetID = assetID,
          AddToArchive = _metadata.Flags.HasFlag(RebuildSiteModelFlags.AddProcessedTagFileToArchive),
          TAGFileName = tagFileName,
          TreatAsJohnDoe = false, // Todo: Determine if this setting has consequences for processing files in the same way as the original model
                                  // Todo: It may be necessary to relate this to the preserved machine information in the model beign rebuilt
          TagFileContent = ms.ToArray()
        });

        // Update the metadata
        _metadata.LastSubmittedTagFile = tagFileKey;
        UpdateMetaData();
      }

      return true;
    }

    /// <summary>
    /// Waits for the process to be complete before advancing to the next phase. Can be cancelled through Abort()
    /// </summary>
    private async Task<bool> ExecuteMonitoring()
    {
      var token = _cancellationSource.Token;
      while (!_aborted || !token.IsCancellationRequested)
      {
        await Task.Delay(kMonitoringDelayMS, token);

        // Check progress...
        // Todo: To be defined
      }

      return !_aborted && !token.IsCancellationRequested;
    }

    /// <summary>
    /// Performs any required clean up when moving into the Completed state.
    /// </summary>
    private async Task ExecuteCompletion()
    {
      // Remove all the collections of TAG file keys
      for (var i = 0; i < _metadata.NumberOfTAGFileKeyCollections; i++)
      {
        var key = new NonSpatialAffinityKey(ProjectUid, i.ToString(CultureInfo.InvariantCulture));
        try
        {
          await FilesCache.RemoveAsync(key);
        }
        catch (KeyNotFoundException)
        {
          _log.LogWarning($"Key {key} not found while removing file key collections for project {ProjectUid}.");
        }
      }
    }

    private async Task<IRebuildSiteModelMetaData> Execute()
    {
      _log.LogInformation($"Starting rebuilding project {ProjectUid}");

      // Get metadata. If one exists and it is 'Complete', then reset it
      _metadata = GetMetaData(ProjectUid);
      var currentPhase = RebuildSiteModelPhase.Unknown;

      if (_metadata != null)
      {
        if (_metadata.Phase == RebuildSiteModelPhase.Complete)
        {
          _log.LogInformation($"Pre-existing completed project rebuild found for {ProjectUid} - resetting");
          // Reset the metadata to start the process
          UpdatePhase(RebuildSiteModelPhase.Unknown);
        }
        else
        {
          _log.LogInformation($"Pre-existing project rebuild found for {ProjectUid} - current state is {_metadata.Phase}");
          currentPhase = _metadata.Phase;
        }
      }

      // Move to the current Phase and start processing from that point

      while (_metadata.Phase != RebuildSiteModelPhase.Complete)
      {
        switch (currentPhase)
        {
          case RebuildSiteModelPhase.Unknown:
            break; // Ignore this phase

          case RebuildSiteModelPhase.Deleting:
            if (!await ExecuteProjectDelete())
                return _metadata;
            break;

          case RebuildSiteModelPhase.Scanning:
            await ExecuteTAGFileScanning();
            break;

          case RebuildSiteModelPhase.Submitting:
            if (!await ExecuteTAGFileSubmission())
              return _metadata;
            break;

          case RebuildSiteModelPhase.Monitoring:
            if (!await ExecuteMonitoring())
              return _metadata;
            break;

          case RebuildSiteModelPhase.Completion:
            await ExecuteCompletion();
            break;
        }

        AdvancePhase(ref currentPhase);
      }

      _metadata.RebuildResult = RebuildSiteModelResult.OK;
      UpdateMetaData();

      return _metadata;
    }

    /// <summary>
    /// Coordinate rebuilding of a project, returning a Tasl for the caller to manahgw.
    /// </summary>
    public Task<IRebuildSiteModelMetaData> ExecuteAsync() => Task.Run(Execute);
  }
}
