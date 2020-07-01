using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.HeartbeatLoggers;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;
using VSS.TRex.SiteModels.Executors;
using VSS.TRex.SiteModels.GridFabric.Listeners;
using VSS.TRex.SiteModels.Heartbeats;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.SiteModels.Interfaces.Listeners;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SiteModelChangeMaps;
using VSS.TRex.SiteModels;

namespace VSS.TRex.Server.ProjectRebuilder
{
  class Program
  {
    static private IStorageProxyCacheCommit CacheFactory(RebuildSiteModelCacheType cacheType)
    {
      return cacheType switch
      {
        RebuildSiteModelCacheType.Metadata =>
          new StorageProxyCache<INonSpatialAffinityKey, IRebuildSiteModelMetaData>(DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Mutable)?
            .GetCache<INonSpatialAffinityKey, IRebuildSiteModelMetaData>(TRexCaches.SiteModelRebuilderMetaDataCacheName())),

        RebuildSiteModelCacheType.KeyCollections =>
          new StorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Mutable)?
            .GetCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>(TRexCaches.SiteModelRebuilderFileKeyCollectionsCacheName())),

        _ => throw new TRexException($"Unknown rebuild site model cache type: {cacheType}")
      };
    }

    private static void DependencyInjection()
    {
      DIBuilder.New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(x => x.AddSingleton<Func<RebuildSiteModelCacheType, IStorageProxyCacheCommit>>((cacheType) => CacheFactory(cacheType)))

        .Build()
        .Add(VSS.TRex.IO.DIUtilities.AddPoolCachesToDI)
        .Add(TRexGridFactory.AddGridFactoriesToDI)
        .Add(VSS.TRex.Storage.Utilities.DIUtilities.AddProxyCacheFactoriesToDI)
        .Build()
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels()))
        .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))

        .Add(x => x.AddSingleton<ITRexHeartBeatLogger, TRexHeartBeatLogger>())
        .Add(x => x.AddSingleton<ITransferProxyFactory, TransferProxyFactory>())
        .Add(x => x.AddSingleton<Func<Guid, bool, TransferProxyType, ISiteModelRebuilder>>(factory => (projectUid, archiveTAGFiles, transferProxyType) => new SiteModelRebuilder(projectUid, archiveTAGFiles, transferProxyType)))
        .Add(x => x.AddSingleton<ISiteModelRebuilderManager, SiteModelRebuilderManager>())
        .Add(x => x.AddSingleton<Func<TransferProxyType, IS3FileTransfer>>(factory => proxyType => new S3FileTransfer(proxyType)))
        .Add(x => x.AddSingleton<IRebuildSiteModelTAGNotifierListener, RebuildSiteModelTAGNotifierListener>())

        .Complete();
    }

    // This static array ensures that all required assemblies are included into the artifacts by the linker
    private static void EnsureAssemblyDependenciesAreLoaded()
    {
      // This static array ensures that all required assemblies are included into the artifacts by the linker
      Type[] AssemblyDependencies =
      {
        typeof(VSS.TRex.DI.DIContext),
        typeof(VSS.TRex.Logging.Logger),
        typeof(VSS.TRex.GridFabric.BaseIgniteClass),
        typeof(VSS.TRex.SiteModels.SiteModel),
        typeof(VSS.TRex.Storage.StorageProxy),
        typeof(VSS.TRex.TAGFiles.GridFabric.NodeFilters.TAGProcessorRoleBasedNodeFilter),
        typeof(VSS.TRex.SiteModelChangeMaps.GridFabric.NodeFilters.SiteModelChangeProcessorRoleBasedNodeFilter)
      };

      foreach (var asmType in AssemblyDependencies)
      {
        if (asmType.FullName == "DummyTypeName")
          Console.WriteLine($"Assembly for type {asmType} has not been loaded.");
      }
    }

    private static async void DoServiceInitialisation()
    {
      // Register the heartbeat loggers
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new MemoryHeartBeatLogger());
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new IgniteNodeMetricsHeartBeatLogger(DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Mutable)));
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new SiteModelRebuilderHeartbeatLogger());

      // Wait until the grid is active
      DIContext.Obtain<IActivatePersistentGridServer>().WaitUntilGridActive(TRexGrids.MutableGridName());

      // Tell the rebuilder manager to find any active rebuilders and start them off from where they left off
      await DIContext.Obtain<ISiteModelRebuilderManager>().BeginOperations();
    }

    static async Task<int> Main()
    {
      try
      {
        EnsureAssemblyDependenciesAreLoaded();
        DependencyInjection();

        var Log = Logging.Logger.CreateLogger<Program>();

        Log.LogInformation("Creating service");
        Log.LogDebug("Creating service");

        var server = new MutableClientServer(new[] { ServerRoles.PROJECT_REBUILDER_ROLE });

        var cancelTokenSource = new CancellationTokenSource();
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
          Console.WriteLine("Exiting");
          DIContext.Obtain<ITRexGridFactory>().StopGrids();
          cancelTokenSource.Cancel();
        };

        DoServiceInitialisation();

        await Task.Delay(-1, cancelTokenSource.Token);
        return 0;
      }
      catch (TaskCanceledException)
      {
        // Don't care as this is thrown by Task.Delay()
        Console.WriteLine("Process exit via TaskCanceledException (SIGTERM)");
        return 0;
      }
      catch (Exception e)
      {
        Console.WriteLine($"Unhandled exception: {e}");
        Console.WriteLine($"Stack trace: {e.StackTrace}");
        return -1;
      }
    }
  }
}
