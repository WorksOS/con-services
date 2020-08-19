using System;
using System.Linq;
using System.Threading;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using Apache.Ignite.Core.Services;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Extensions;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Services;
using VSS.TRex.SiteModelChangeMaps.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Services;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.SiteModelChangeMaps.GridFabric.Services
{
  /// <summary>
  /// The Ignite deployed service that processes site model change notifications (representing the set of sub grid spatial data
  /// that changed as a result of processing a set of TAG file data) and representing those changes in the per-machine tracking
  /// to enable machine to request data with 'only data that changed since last visit' semantics
  /// </summary>
  public class SiteModelChangeProcessorService : BaseService, IService, ISiteModelChangeProcessorService
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SiteModelChangeProcessorService>();

    private const byte VERSION_NUMBER = 1;

    private const int DEFAULT_SERVICE_CHECK_INTERVAL = 1000;

    /// <summary>
    /// The interval between epochs where the service checks to see if there is anything to do
    /// </summary>
    private int _serviceCheckIntervalMs = DEFAULT_SERVICE_CHECK_INTERVAL;

    /// <summary>
    /// Flag set then Cancel() is called to instruct the service to finish operations
    /// </summary>
    public bool Aborted { get; private set; }

    /// <summary>
    /// Notes that the service has completed processing of the queue content resent at the start of service
    /// execution and is now processing items as they arrive in the queue
    /// </summary>
    public bool InSteadyState { get; private set; }

    /// <summary>
    /// The event wait handle used to mediate sleep periods between operation epochs of the service
    /// </summary>
    private EventWaitHandle _waitHandle;

    /// <summary>
    /// Default no-args constructor
    /// </summary>
    public SiteModelChangeProcessorService()
    {
    }

    /// <summary>
    /// Initializes the service ready for accessing buffered site model spatial changer sets and providing them to processing contexts
    /// </summary>
    public void Init(IServiceContext context)
    {
      if (_log == null)
      {
        Console.WriteLine($"Error: Null logger present in {nameof(SiteModelChangeProcessorService)}.{nameof(Init)}");
      }

      _log.LogInformation($"{nameof(SiteModelChangeProcessorService)} {context.Name} initializing");
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
          Console.WriteLine($"Error: Null logger present in {nameof(SiteModelChangeProcessorService)}.{nameof(Execute)}");
        }

        try
        {
          _log.LogInformation($"{context.Name} starting executing");
          Aborted = false;
          _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

          // Get the ignite grid and cache references

          var ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(StorageMutability.Immutable) ??
                       Ignition.GetIgnite(TRexGrids.ImmutableGridName());

          if (ignite == null)
          {
            _log.LogError("Ignite reference in service is null - aborting service execution");
            return;
          }

          var queueCache = ignite.GetCache<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>(TRexCaches.SiteModelChangeBufferQueueCacheName());

          _log.LogInformation($"Obtained queue cache for SiteModelChangeBufferQueueKey: {queueCache}");

          var handler = new SiteModelChangeProcessorItemHandler();
          var listener = new LocalSiteModelChangeListener(handler);

          // Obtain the query handle for the continuous query from the DI context, or if not available create it directly
          // Construct the continuous query machinery
          // Set the initial query to return all elements in the cache
          // Instantiate the queryHandle and start the continuous query on the remote nodes
          // Note: Only cache items held on this local node will be handled here
          var queryHandleFactory = DIContext.Obtain<Func<LocalSiteModelChangeListener, IContinuousQueryHandle<ICacheEntry<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>>>>();
          IContinuousQueryHandle<ICacheEntry<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>> queryHandle = null;

          if (queryHandleFactory != null)
          {
            _log.LogInformation("Obtaining query handle from DI factory");
            queryHandle = queryHandleFactory(listener);
          }

          if (queryHandle == null)
          {
            _log.LogInformation("Obtaining query handle from QueryContinuous() API");

            queryHandle = queueCache.QueryContinuous
            (qry: new ContinuousQuery<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem>(listener) { Local = true },
              initialQry: new ScanQuery<ISiteModelChangeBufferQueueKey, ISiteModelChangeBufferQueueItem> { Local = true });
          }

          using (queryHandle)
          {
            _log.LogInformation("Performing initial continuous query cursor scan of items to process");

            // Perform the initial query to grab all existing elements and process them. Make sure to sort them in time order first
            queryHandle.GetInitialQueryCursor().OrderBy(x => x.Key.InsertUTCTicks).ForEach(handler.Add);

            while (!Aborted)
            {
              try
              {
                {
                  // Cycle looking for new work to do as items arrive until aborted...
                  _log.LogInformation("Entering steady state continuous query scan of items to process");

                  // Activate the handler with the inject initial continuous query and move into steady state processing

                  InSteadyState = true;
                  handler.Activate();
                  do
                  {
                    _waitHandle.WaitOne(_serviceCheckIntervalMs);
                  } while (!Aborted);
                }
              }
              catch (Exception e)
              {
                _log.LogError(e, "Site model change processor service unhandled exception, waiting and trying again");

                // Sleep for 5 seconds to see if things come right and then try again
                Thread.Sleep(5000);
              }
            }
          }
        }
        catch (Exception e)
        {
          _log.LogError(e, "Exception occured performing initial set up of conitunous query and scan of existing items");
        }
      }
      finally
      {
        _log.LogInformation($"{context.Name} completed executing");
      }
    }

    /// <summary>
    /// Cancels the current operation context of the service
    /// </summary>
    public void Cancel(IServiceContext context)
    {
      _log.LogInformation($"{context.Name} cancelling");

      Aborted = true;
      _waitHandle?.Set();
    }

    /// <summary>
    /// The service has no serialization requirements
    /// </summary>
    public override void ToBinary(IBinaryRawWriter writer)
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
          Console.WriteLine($"Error: Exception serializing site model change service state {e.Message} occured at {e.StackTrace}");
        }
        else
        {
          _log.LogError(e, "Exception serializing site model change service state");
        }
      }
    }

    /// <summary>
    /// The service has no serialization requirements
    /// </summary>
    public override void FromBinary(IBinaryRawReader reader)
    {
      try
      {
        VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

        _serviceCheckIntervalMs = reader.ReadInt();
      }
      catch (Exception e)
      {
        if (_log == null)
        {
          Console.WriteLine("Error: No logger available");
          Console.WriteLine($"Error: Exception deserializing site model change service state {e.Message} occured at {e.StackTrace}");
        }
        else
        {
          _log.LogError(e, "Exception deserializing site model change service state");
        }

        _serviceCheckIntervalMs = DEFAULT_SERVICE_CHECK_INTERVAL;
      }
    }
  }
}
