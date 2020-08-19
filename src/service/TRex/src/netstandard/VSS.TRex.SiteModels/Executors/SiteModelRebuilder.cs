using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.Common.Abstractions.Configuration;
using VSS.Serilog.Extensions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Utilities;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.SiteModels.Executors
{
  /// <summary>
  /// Defines and manages the activities executed asynchronously to rebuild a project. This class is instantiated
  /// by the project rebuilder manager for each projects being rebuilt
  /// </summary>
  public class SiteModelRebuilder : ISiteModelRebuilder
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SiteModelRebuilder>();

    private const int MONITORING_DELAY_MS = 10000;
      
    /// <summary>
    /// Length of time to wait between monitoring epochs
    /// </summary>
    private readonly int _monitoringDelayMs = DIContext.Obtain<IConfigurationStore>().GetValueInt("REBUILD_SITE_MODEL_MONITORING_INTERVAL_MS", MONITORING_DELAY_MS);

    /// <summary>
    /// The key name to be used for the metadata entries in the metadata cache (one per project)
    /// </summary>
    public static readonly string MetadataKeyName = "metadata";

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

    private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();

    private IS3FileTransfer _s3FileTransfer;

    public SiteModelRebuilder(Guid projectUid, bool archiveTagFiles, TransferProxyType originS3TransferProxy)
    {
      ProjectUid = projectUid;

      var flags = archiveTagFiles ? RebuildSiteModelFlags.AddProcessedTagFileToArchive : 0;

      _metadata = new RebuildSiteModelMetaData
      {
        ProjectUID = projectUid,
        Flags = flags,
        OriginS3TransferProxy = originS3TransferProxy
      };
    }

    public SiteModelRebuilder(IRebuildSiteModelMetaData metadata)
    {
      ProjectUid = metadata.ProjectUID;
      _metadata = metadata;
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
    /// Reads the rebuild site model metadata for the given project from the persistent cache
    /// </summary>
    private IRebuildSiteModelMetaData GetMetaData(Guid projectUid)
    {
      try
      {
        return MetadataCache.Get(new NonSpatialAffinityKey(projectUid, MetadataKeyName));
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
      lock (_metadata)
      {
        _metadata.LastUpdateUtcTicks = DateTime.UtcNow.Ticks;
        MetadataCache.Put(new NonSpatialAffinityKey(ProjectUid, MetadataKeyName), _metadata);
      }
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
    private static RebuildSiteModelPhase NextPhase(RebuildSiteModelPhase phase)
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
    /// Moves to the next processing phase given the current phase
    /// </summary>
    private void AdvancePhase(ref RebuildSiteModelPhase currentPhase)
    {
      currentPhase = NextPhase(currentPhase);

      _log.LogInformation($"Advancing to phase: {currentPhase}");

      UpdatePhase(currentPhase);
    }

    /// <summary>
    /// Ensures there is no current rebuilding activity for this project.
    /// The exception is when there exists a metadata record for the project and the phase is complete
    /// </summary>
    public bool ValidateNoActiveRebuilderForProject(Guid projectUid)
    {
      var metadata = GetMetaData(projectUid);

      return metadata == null || metadata.Phase == RebuildSiteModelPhase.Complete;
    }

    /// <summary>
    /// Performs a partial deletion of the site model ready for data to be reprocessed into it.
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
    /// Scans the source S3 location for all tag files to be submitted and places these names into a cache to 
    /// be used as the source for the TAG files submission phase.
    /// </summary>
    private async Task<bool> ExecuteTAGFileScanning()
    {
      _log.LogInformation("Scanning files present in S3 and placing them in the files cache");

      var continuation = string.Empty;
      do
      {
        if (_aborted)
          return false;

        var (candidateTagFiles, nextContinuation) = await _s3FileTransfer.ListKeys($"/{_metadata.ProjectUID}", 1000, continuation);
        continuation = nextContinuation;

        // Put the candidate TAG files into the cache
        var sb = new StringBuilder(2 * (candidateTagFiles.Sum(x => x.Length) + 1));
        sb.AppendJoin('|', candidateTagFiles);

        await using var ms = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        await using var compressedStream = MemoryStreamCompression.Compress(ms);

        _log.LogInformation($"Putting block of {candidateTagFiles.Length} TAG file names for project {ProjectUid}");
        await FilesCache.PutAsync(new NonSpatialAffinityKey(ProjectUid, _metadata.NumberOfTAGFileKeyCollections.ToString(CultureInfo.InvariantCulture)), new SerialisedByteArrayWrapper(compressedStream.ToArray()));

        _metadata.NumberOfTAGFilesFromS3 += candidateTagFiles.Length;
        _metadata.NumberOfTAGFileKeyCollections++;

        UpdateMetaData();
      } while (!string.IsNullOrEmpty(continuation));

      return true;
    }

    /// <summary>
    /// Iterates over all elements stored in the file cache and requests those files from 3, submitting them
    /// to the TAG file processor as it goes.
    /// </summary>
    private async Task<bool> ExecuteTAGFileSubmission()
    {
      var tagFileCollection = new List<string>();

      _log.LogInformation("Reading collections from files cache prior to submission");

      // Read all collections into a single list
      for (var i = 0; i < _metadata.NumberOfTAGFileKeyCollections; i++)
      {
        if (_aborted)
          return false;

        var key = new NonSpatialAffinityKey(ProjectUid, i.ToString(CultureInfo.InvariantCulture));
        try
        {
          await using var ms = new MemoryStream((await FilesCache.GetAsync(key)).Bytes);
          var uncompressedTagFileCollection = MemoryStreamCompression.Decompress(ms);

          if (uncompressedTagFileCollection.Length > 0) // Check the collection is not empty
          {
            tagFileCollection.AddRange(Encoding.UTF8.GetString(uncompressedTagFileCollection.ToArray()).Split('|'));
          }
          else
          {
            _log.LogInformation($"TAG file key collection {i} had no members");
          }
        }
        catch (KeyNotFoundException e)
        {
          _log.LogError(e, $"Key {key} not found. Aborting.");
          _metadata.RebuildResult = RebuildSiteModelResult.UnableToLocateTAGFileKeyCollection;
          UpdateMetaData();
          return false;
        }
      }

      _log.LogInformation($"Read {_metadata.NumberOfTAGFileKeyCollections} containing {tagFileCollection.Count} files. About to sort and submit");

      // Sort the list based on time contained in the name of the tag file
      tagFileCollection = tagFileCollection.OrderBy(x =>
      {
        var keySplit = x.Split('/');
        var fileSplit = keySplit[2].Split("--");

        return fileSplit.Length == 3 ? Convert.ToUInt64(fileSplit[2]) : 0;
      }).ToList();

      var submittedCount = 0;

      // Iterate across the sorted collection submitting each one in turn to the TAG file processor. 
      // After successful submission update metadata with name of submitted file
      foreach (var tagFileKey in tagFileCollection)
      {
        if (_aborted)
          return false;

        // Make some determination that the key looks valid and defines a *.tag file
        // TODO - complete this check

        submittedCount++;

        if (_metadata.NumberOfTAGFilesSubmitted >= submittedCount)
          continue;

        // Determine the asset ID from the key of form '<{MachineUid}>/<TagFileName>'
        var split = tagFileKey.Split('/');
        var assetUid = Guid.Parse(split[1]);
        var tagFileName = split[2];

        // Read the content of the TAG file from S3
        var tagFile = await _s3FileTransfer.Proxy.Download(tagFileKey);
        await using var ms = new MemoryStream();
        await tagFile.FileStream.CopyToAsync(ms);

        // Submit the file...
        var submissionRequest = new SubmitTAGFileRequest();
        var submissionResult = await submissionRequest.ExecuteAsync(new SubmitTAGFileRequestArgument
        {
          ProjectID = ProjectUid,
          AssetID = assetUid,
          SubmissionFlags = _metadata.Flags.HasFlag(RebuildSiteModelFlags.AddProcessedTagFileToArchive) ? TAGFileSubmissionFlags.AddToArchive : TAGFileSubmissionFlags.None |
                  TAGFileSubmissionFlags.NotifyRebuilderOnProceesing,
          TAGFileName = tagFileName,
          TreatAsJohnDoe = assetUid.Equals(Guid.Empty),
          TagFileContent = ms.ToArray(),
        });

        // Update the metadata
        _metadata.LastSubmittedTagFile = tagFileKey;
        _metadata.NumberOfTAGFilesSubmitted = submittedCount;
        UpdateMetaData();
      }

      return true;
    }

    /// <summary>
    /// Waits for the process to be complete before advancing to the next phase. Can be cancelled through Abort()
    /// </summary>
    private async Task<bool> ExecuteMonitoring()
    {
      _log.LogInformation("Entering monitoring state");

      var token = _cancellationSource.Token;
      while (!_aborted && !token.IsCancellationRequested)
      {
        await Task.Delay(_monitoringDelayMs, token);

        // Check progress
        if (_metadata.NumberOfTAGFilesProcessed >= _metadata.NumberOfTAGFilesFromS3)
        {
          // Finished!
          return true;
        }
      }

      return !_aborted && !token.IsCancellationRequested;
    }

    /// <summary>
    /// Performs any required clean up when moving into the Completed state.
    /// </summary>
    private async Task<bool> ExecuteCompletion()
    {
      _log.LogInformation("Executing completion activities");

      // Remove all the collections of TAG file keys
      for (var i = 0; i < _metadata.NumberOfTAGFileKeyCollections; i++)
      {
        if (_aborted)
          return false;

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

      return true;
    }

    private async Task<IRebuildSiteModelMetaData> Execute()
    {
      try
      {
        _log.LogInformation($"Starting rebuilding project {ProjectUid}");

        var currentPhase = RebuildSiteModelPhase.Unknown;

        // Get metadata. If one exists and it is 'Complete', then reset it
        var persistedMetadata = GetMetaData(ProjectUid);
        if (persistedMetadata != null)
        {
          if (persistedMetadata.Phase == RebuildSiteModelPhase.Complete)
          {
            _log.LogInformation($"Pre-existing completed project rebuild found for {ProjectUid} - resetting");
            // Reset the metadata to start the process
            UpdatePhase(RebuildSiteModelPhase.Unknown);
          }
          else
          {
            _log.LogInformation($"Pre-existing project rebuild found for {ProjectUid} - current state is {_metadata.Phase}");
            currentPhase = persistedMetadata.Phase;
          }

          // Set the internal meta data to the state of the persisted metadata
          _metadata = persistedMetadata;
        }

        // Ensure persisted metadata state matches the internal metadata state
        UpdateMetaData();

        _s3FileTransfer = DIContext.Obtain<Func<TransferProxyType, IS3FileTransfer>>()(_metadata.OriginS3TransferProxy);

        // Move to the current Phase and start processing from that point

        while (!_aborted && _metadata.Phase != RebuildSiteModelPhase.Complete)
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
              if (!await ExecuteTAGFileScanning())
                return _metadata;
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
              if (!await ExecuteCompletion())
                return _metadata;
              break;
          }

          AdvancePhase(ref currentPhase);
        }

        _metadata.RebuildResult = _aborted ? RebuildSiteModelResult.Aborted : RebuildSiteModelResult.OK;
        UpdateMetaData();
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Exception occurred while in rebuilding phase {_metadata?.Phase ?? RebuildSiteModelPhase.Unknown}");

        if (_metadata != null)
        {
          _metadata.RebuildResult = RebuildSiteModelResult.UnhandledException;
          UpdateMetaData();
        }
      }

      return _metadata;
    }

    /// <summary>
    /// Coordinate rebuilding of a project, returning a Task for the caller to manage.
    /// </summary>
    public Task<IRebuildSiteModelMetaData> ExecuteAsync() => Task.Run(Execute);

    /// <summary>
    /// Notifies the rebuilder that a TAG file marked with notify after processing has been processed
    /// </summary>
    public void TAGFilesProcessed(IProcessTAGFileResponseItem[] responseItems)
    {
      // Make a note of the last file processed
      if (responseItems.Length > 0)
        _metadata.LastProcessedTagFile = responseItems[^1].FileName;

      // Update the count of observed files processed
      _metadata.NumberOfTAGFilesProcessed += responseItems.Length;

      UpdateMetaData();
    }
  }
}
