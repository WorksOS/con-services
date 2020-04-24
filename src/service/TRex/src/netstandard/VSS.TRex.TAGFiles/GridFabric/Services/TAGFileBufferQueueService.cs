using Apache.Ignite.Core.Services;
using Microsoft.Extensions.Logging;
using System.Threading;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Services;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Models;
using System;

namespace VSS.TRex.TAGFiles.GridFabric.Services
{
  /// <summary>
  /// Service metaphor providing access and management control over designs stored for site models
  /// </summary>
  public class TAGFileBufferQueueService : BaseService, IService, ITAGFileBufferQueueService
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<TAGFileBufferQueueService>();

    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The interval between epochs where the service checks to see if there is anything to do
    /// </summary>
    private int serviceCheckIntervalMS = 1000;

    /// <summary>
    /// Flag set then Cancel() is called to instruct the service to finish operations
    /// </summary>
    private bool aborted;

    /// <summary>
    /// The event wait handle used to mediate sleep periods between operation epochs of the service
    /// </summary>
    private EventWaitHandle waitHandle;

    /// <summary>
    /// Default no-args constructor that tailors this service to apply to TAG processing node in the mutable data grid
    /// </summary>
    public TAGFileBufferQueueService()
    {
    }

    /// <summary>
    /// Initializes the service ready for accessing buffered TAG files and providing them to processing contexts
    /// </summary>
    /// <param name="context"></param>
    public void Init(IServiceContext context)
    {
      Log.LogInformation($"{nameof(TAGFileBufferQueueService)} {context.Name} initializing");
    }

    /// <summary>
    /// Executes the life cycle of the service until it is aborted
    /// </summary>
    /// <param name="context"></param>
    public void Execute(IServiceContext context)
    {
      try
      {
        Log.LogInformation($"{nameof(TAGFileBufferQueueService)} {context.Name} starting executing");

        aborted = false;
        waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        // Get the ignite grid and cache references

        var _ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Mutable) ??
                      Ignition.GetIgnite(TRexGrids.MutableGridName());

        if (_ignite == null)
        {
          Log.LogError("Ignite reference in service is null - aborting service execution");
          return;
        }

        var queueCache =
          _ignite.GetCache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(TRexCaches.TAGFileBufferQueueCacheName());

        var handler = new TAGFileBufferQueueItemHandler();

        // Construct the continuous query machinery
        // Set the initial query to return all elements in the cache
        // Instantiate the queryHandle and start the continuous query on the remote nodes
        // Note: Only cache items held on this local node will be handled here
        // var = IContinuousQueryHandle<ICacheEntry<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>>
        using (var queryHandle = queueCache.QueryContinuous
        (qry: new ContinuousQuery<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(new LocalTAGFileListener(handler)) {Local = true},
          initialQry: new ScanQuery<ITAGFileBufferQueueKey, TAGFileBufferQueueItem> {Local = true}))
        {
          Log.LogInformation(
            "Performing initial continuous query cursor scan of items to process in TAGFileBufferQueue");

          // Perform the initial query to grab all existing elements and add them to the grouper
          foreach (var item in queryHandle.GetInitialQueryCursor())
          {
            handler.Add(item.Key);
          }

          // Cycle looking for new work to do as TAG files arrive until aborted...
          Log.LogInformation("Entering steady state continuous query scan of items to process in TAGFileBufferQueue");

          do
          {
            waitHandle.WaitOne(serviceCheckIntervalMS);
            //Log.LogInformation("Continuous query scan of items to process in TAGFileBufferQueue still active");
          } while (!aborted);
        }
      }
      catch(Exception e)
      {
        Log.LogError(e, "Tag file buffer service unhandled exception");
      }
      finally
      {
        Log.LogInformation($"{nameof(TAGFileBufferQueueService)} {context.Name} completed executing");
      }
    }

    /// <summary>
    /// Cancels the current operation context of the service
    /// </summary>
    /// <param name="context"></param>
    public void Cancel(IServiceContext context)
    {
      Log.LogInformation($"{nameof(TAGFileBufferQueueService)} {context.Name} cancelling");

      aborted = true;
      waitHandle?.Set();
    }

    /// <summary>
    /// The service has no serialization requirements
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt(serviceCheckIntervalMS);
    }

    /// <summary>
    /// The service has no serialization requirements
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      serviceCheckIntervalMS = reader.ReadInt();
    }
  }
}
