using Apache.Ignite.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Services;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.GridFabric.Services
{
  /// <summary>
  /// Service metaphor providing access and management control over designs stored for site models
  /// </summary>
  public class SegmentRetirementQueueService : BaseService, IService, ISegmentRetirementQueueService
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SegmentRetirementQueueService>();

    private static readonly bool _reportDetailedSegmentRetirementActivityToLog = false;

    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The interval between epochs where the service checks to see if there is anything to do. Set to 30 seconds.
    /// </summary>
    private const int SEGMENT_RETIREMENT_QUEUE_SERVICE_CHECK_INTERVAL_MS = 30000;

    /// <summary>
    /// Flag set then Cancel() is called to instruct the service to finish operations
    /// </summary>
    private bool _aborted;

    /// <summary>
    /// The event wait handle used to mediate sleep periods between operation epochs of the service
    /// </summary>
    private EventWaitHandle _waitHandle;

    // Todo: Set the retirement age from the environment/configuration
    public TimeSpan retirementAge = new TimeSpan(0, 10, 0); // Set to 10 minutes as a maximum consistency window

    /// <summary>
    /// Default no-args constructor that tailors this service to apply to TAG processing node in the mutable data grid
    /// </summary>
    public SegmentRetirementQueueService()
    {
    }

    /// <summary>
    /// Initializes the service ready for accessing segment keys
    /// </summary>
    public void Init(IServiceContext context)
    {
      _log.LogInformation($"{nameof(SegmentRetirementQueueService)} {context.Name} initializing");
    }

    /// <summary>
    /// Executes the life cycle of the service until it is aborted
    /// </summary>
    public void Execute(IServiceContext context)
    {
      _log.LogInformation($"{nameof(SegmentRetirementQueueService)} {context.Name} starting executing");

      _aborted = false;
      _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

      // Get the ignite grid and cache references

      var mutableIgnite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Mutable) ?? Ignition.GetIgnite(TRexGrids.MutableGridName());

      if (mutableIgnite == null)
      {
        _log.LogError("Mutable Ignite reference in service is null - aborting service execution");
        return;
      }

      var queueCache = mutableIgnite.GetCache<ISegmentRetirementQueueKey, SegmentRetirementQueueItem>(TRexCaches.TAGFileBufferQueueCacheName());

      var queue = new SegmentRetirementQueue();
      var handler = new SegmentRetirementQueueItemHandler();

      // Cycle looking for new work to do until aborted...
      do
      {
        try
        {
          // Obtain a specific local mutable storage proxy so as to have a local transactional proxy
          // for this activity
          var storageProxy = DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage();

          if (storageProxy.Mutability != StorageMutability.Mutable)
          {
            throw new TRexException("Non mutable storage proxy available to segment retirement queue");
          }

          _log.LogInformation("About to query retiree spatial streams from cache");

          var earlierThan = DateTime.UtcNow - retirementAge;
          // Retrieve the list of segments to be retired
          var retirees = queue.Query(earlierThan);

          // Pass the list to the handler for action
          var retireesCount = retirees?.Count ?? 0;
          if (retireesCount > 0)
          {
            _log.LogInformation($"About to retire {retireesCount} groups of spatial streams from mutable and immutable contexts");

            if (handler.Process(storageProxy, queueCache, retirees))
            {
              if (_reportDetailedSegmentRetirementActivityToLog)
                _log.LogInformation($"Successfully retired {retireesCount} spatial streams from mutable and immutable contexts");

              // Remove the elements from the segment retirement queue
              queue.Remove(earlierThan);
            }
            else
            {
              _log.LogError($"Failed to retire {retireesCount} spatial streams from mutable and immutable contexts");
            }
          }
        }
        catch (Exception e)
        {
          _log.LogError(e, "Exception reported while obtaining new group of retirees to process:");
        }

        _waitHandle.WaitOne(SEGMENT_RETIREMENT_QUEUE_SERVICE_CHECK_INTERVAL_MS);
      } while (!_aborted);

      _log.LogInformation($"{nameof(SegmentRetirementQueueService)} {context.Name} completed executing");
    }

    /// <summary>
    /// Cancels the current operation context of the service
    /// </summary>
    public void Cancel(IServiceContext context)
    {
      _log.LogInformation($"{nameof(SegmentRetirementQueueService)} {context.Name} cancelling");

      _aborted = true;
      _waitHandle?.Set();
    }

    /// <summary>
    /// The service has no serialization requirements
    /// </summary>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteLong(retirementAge.Ticks);
    }

    /// <summary>
    /// The service has no serialization requirements
    /// </summary>
    public override void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      retirementAge = new TimeSpan(reader.ReadLong());
    }
  }
}
