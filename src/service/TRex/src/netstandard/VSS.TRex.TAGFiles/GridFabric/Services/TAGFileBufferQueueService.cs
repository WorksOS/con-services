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
    private static readonly ILogger _log = Logging.Logger.CreateLogger<TAGFileBufferQueueService>();

    private const byte VERSION_NUMBER = 1;
    private const int DEFAULT_SERVICE_CHECK_INTERVAL = 1000;

    /// <summary>
    /// The interval between epochs where the service checks to see if there is anything to do
    /// </summary>
    private int _serviceCheckIntervalMs = DEFAULT_SERVICE_CHECK_INTERVAL;

    /// <summary>
    /// Flag set then Cancel() is called to instruct the service to finish operations
    /// </summary>
    private bool _aborted;

    /// <summary>
    /// The event wait handle used to mediate sleep periods between operation epochs of the service
    /// </summary>
    private EventWaitHandle _waitHandle;

    /// <summary>
    /// The handler responsible for coordinating items from the TAG file buffer queue and the processing contexts
    /// </summary>
    private TAGFileBufferQueueItemHandler _handler;

    /// <summary>
    /// Default no-args constructor that tailors this service to apply to TAG processing node in the mutable data grid
    /// </summary>
    public TAGFileBufferQueueService()
    {
    }

    /// <summary>
    /// Initializes the service ready for accessing buffered TAG files and providing them to processing contexts
    /// </summary>
    public void Init(IServiceContext context)
    {
      if (_log == null)
      {
        Console.WriteLine($"Error: Null logger present in {nameof(TAGFileBufferQueueService)}.{nameof(Init)}");
      }

      _log.LogInformation($"{nameof(TAGFileBufferQueueService)} {context.Name} initializing");
    }

    /// <summary>
    /// Executes the life cycle of the service until it is aborted
    /// </summary>
    public void Execute(IServiceContext context)
    {
      try
      {
        if (_log == null)
        {
          Console.WriteLine($"Error: Null logger present in {nameof(TAGFileBufferQueueService)}.{nameof(Execute)}");
        }

        _log.LogInformation($"{nameof(TAGFileBufferQueueService)} {context.Name} starting executing");

        _aborted = false;
        _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        // Get the ignite grid and cache references

        var ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Mutable) ??
                     Ignition.GetIgnite(TRexGrids.MutableGridName());

        if (ignite == null)
        {
          _log.LogError("Ignite reference in service is null - aborting service execution");
          return;
        }

        var queueCache = ignite.GetCache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(TRexCaches.TAGFileBufferQueueCacheName());

        _handler = new TAGFileBufferQueueItemHandler();

        // Construct the continuous query machinery
        // Set the initial query to return all elements in the cache
        // Instantiate the queryHandle and start the continuous query on the remote nodes
        // Note: Only cache items held on this local node will be handled here
        using var queryHandle = queueCache.QueryContinuous
        (qry: new ContinuousQuery<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(new LocalTAGFileListener(_handler)) {Local = true},
          initialQry: new ScanQuery<ITAGFileBufferQueueKey, TAGFileBufferQueueItem> {Local = true});

        _log.LogInformation("Performing initial continuous query cursor scan of items to process in TAGFileBufferQueue");

        // Perform the initial query to grab all existing elements and add them to the grouper
        foreach (var item in queryHandle.GetInitialQueryCursor())
        {
          _handler.Add(item.Key);
        }

        // Transition into steady state looking for new elements in the cache via the continuous query
        while (!_aborted)
        {
          try
          {

            // Cycle looking for new work to do as TAG files arrive until aborted...
            _log.LogInformation("Entering steady state continuous query scan of items to process in TAGFileBufferQueue");

            do
            {
              _waitHandle.WaitOne(_serviceCheckIntervalMs);
              //Log.LogInformation("Continuous query scan of items to process in TAGFileBufferQueue still active");
            } while (!_aborted);
          }
          catch (Exception e)
          {
            _log.LogError(e, "Tag file buffer service unhandled exception, waiting and trying again");

            // Sleep for 5 seconds to see if things come right and then try again
            Thread.Sleep(5000);
          }
        }

        _handler.Cancel();
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occurred performing initial set up of continuous query and scan of existing items");
      }
      finally
      {
        _log.LogInformation($"{nameof(TAGFileBufferQueueService)} {context.Name} completed executing");
      }
    }

    /// <summary>
    /// Cancels the current operation context of the service
    /// </summary>
    public void Cancel(IServiceContext context)
    {
      try
      {
        _log.LogInformation($"{nameof(TAGFileBufferQueueService)} {context.Name} cancelling");


        _aborted = true;
        _waitHandle?.Set();
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception cancelling TAG file buffer queue service");
      }
    }

    /// <summary>
    /// The service has no serialization requirements
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      try
      {
        VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

        writer.WriteInt(_serviceCheckIntervalMs);
      }
      catch (Exception e)
      {
        if (_log == null)
        {
          Console.WriteLine("Error: No logger available");
          Console.WriteLine($"Error: Exception serializing TAG file buffer queue state {e.Message} occurred at {e.StackTrace}");
        }
        else
        {
          _log.LogError(e, "Exception serializing TAG file buffer queue state");
        }
      }
    }

    /// <summary>
    /// The service has no serialization requirements
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      try
      {
        var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

        if (version == 1)
        {
          _serviceCheckIntervalMs = reader.ReadInt();
        }
      }
      catch (Exception e)
      {
        if (_log == null)
        {
          Console.WriteLine("Error: No logger available");
          Console.WriteLine($"Error: Exception deserializing TAG file buffer queue state {e.Message} occurred at {e.StackTrace}");
        }
        else
        {
          _log.LogError(e, "Exception deserializing TAG file buffer queue state");
        }

        _serviceCheckIntervalMs = DEFAULT_SERVICE_CHECK_INTERVAL;
      }
    }
  }
}
