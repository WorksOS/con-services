using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.Serilog.Extensions;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.IO.Helpers;
using VSS.TRex.SiteModels.Executors;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Utilities;

namespace VSS.TRex.SiteModels
{
  /// <summary>
  /// Defines and manages the activites executed asynchronously to rebuild a project. This class is instantiated
  /// by the project rebuilder manager for each project sbeing rebuilt
  /// </summary>
  public class SiteModelRebuilder
  {
    private static ILogger _log = Logging.Logger.CreateLogger<SiteModelRebuilder>();

    /// <summary>
    /// Length of time to wait between monitoring epochs
    /// </summary>
    private const int kMonitoringDelayMS = 10000;

    /// <summary>
    /// The storage proxy cache for the rebuilder to use for tracking metadata
    /// </summary>
    private IStorageProxyCache<Guid, IRebuildSiteModelMetaData> _metadataCache = DIContext.Obtain<Func<IStorageProxyCache<Guid, IRebuildSiteModelMetaData>>>()();

    /// <summary>
    /// The storage proxy cache for the rebuilder to use to store names of TAG files requested from S3
    /// </summary>
    private IStorageProxyCache<(Guid, int), ISerialisedByteArrayWrapper> _filesCache = DIContext.Obtain<Func<IStorageProxyCache<(Guid, int), ISerialisedByteArrayWrapper>>>()();

    /// <summary>
    /// Project ID of this project this rebuilder is managing
    /// </summary>
    public Guid ProjectUid { get; }

    /// <summary>
    /// The response tht will be provided to the caller when complete
    /// </summary>
    private RebuildSiteModelRequestResponse _response;

    private IRebuildSiteModelMetaData _metadata = new RebuildSiteModelMetaData();

    private bool _aborted = false;

    private CancellationTokenSource _cancellationSource = new CancellationTokenSource();

    public SiteModelRebuilder(Guid projectUid)
    {
      ProjectUid = projectUid;

      _response = new RebuildSiteModelRequestResponse(projectUid);
    }

    public void Abort()
    {
      _aborted = true;
      _cancellationSource.Cancel();
    }

    private IRebuildSiteModelMetaData GetMetaData(Guid projectUid)
    {
      try
      {
        return _metadataCache.Get(projectUid);
      }
      catch (KeyNotFoundException)
      {
        // No metadata present - just return null
        return null;
      }
    }

    private void UpdateMetaData()
    {
      _metadata.LastUpdateUtcTicks = DateTime.UtcNow.Ticks;
      _metadataCache.Put(ProjectUid, _metadata);

    }
    private void UpdatePhase(RebuildSiteModelPhase phase)
    {
      _metadata.Phase = phase;
      UpdateMetaData();
    }

    private void AdvancePhase(ref RebuildSiteModelPhase currentPhase)
    {
      UpdatePhase(currentPhase++);
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
        var (candidateTAGFiles, nextContinuation) = await s3FileTransfer.ListKeys($"/{_metadata.ProjectUid}", 1000, continuation);
        continuation = nextContinuation;
        runningCount += candidateTAGFiles.Count;

        // Put the candidate TAG files into the cache
        var sb = new StringBuilder(2 * (candidateTAGFiles.Sum(x => x.Length) + 1));
        sb.AppendJoin('|', candidateTAGFiles);

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        using (var compressedStream = MemoryStreamCompression.Compress(ms))
        {
          if (_log.IsTraceEnabled())
            _log.LogInformation($"Putting block of {candidateTAGFiles.Count} TAG file names for project {ProjectUid}");
          _filesCache.Put((ProjectUid, runningCount), new SerialisedByteArrayWrapper(compressedStream.ToArray()));
        }
      } while (!string.IsNullOrEmpty(continuation));

      _metadata.NumberOfTAGFilesFromS3 = runningCount;
    }

    /// <summary>
    /// Iterates over all elements stored in the file cache and requests those files from 3, submitting them
    /// to the TAG file processor as it goes.
    /// </summary>
    private void ExecuteTAGFileSubmission()
    {
    }

    /// <summary>
    /// Waits for the process to be complete before advancing to the next phase
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
    private void ExecuteCompletion()
    {
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
            ExecuteTAGFileSubmission();
            break;

          case RebuildSiteModelPhase.Monitoring:
            ExecuteMonitoring();
            break;
        }

        AdvancePhase(ref currentPhase);
      }

      if (currentPhase == RebuildSiteModelPhase.Complete)
        ExecuteCompletion();

      return _metadata;
    }

    /// <summary>
    /// Coordinate rebuilding of a project, returning a Tasl for the caller to manahgw.
    /// </summary>
    public Task<IRebuildSiteModelMetaData> ExecuteAsync()
    {
      return Task.Run(() => Execute());
    }
  }
}
