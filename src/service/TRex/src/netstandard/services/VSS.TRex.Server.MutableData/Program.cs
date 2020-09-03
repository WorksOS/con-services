using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CCSS.CWS.Client;
using CoreX.Interfaces;
using CoreX.Wrapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Cache.MemoryCache;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.TagFileAuth.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.Proxy;
using VSS.TRex.Alignments;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.HeartbeatLoggers;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.GridFabric.Factories;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Servers.Compute;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Executors;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.Models;
using VSS.WebApi.Common;

namespace VSS.TRex.Server.MutableData
{
  public class Program
  {
    private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Build()
        .Add(x => x.AddSingleton<ICoreXWrapper, CoreXWrapper>())
        .Add(x => x.AddSingleton<IACSTranslator, ACSTranslator>())
        .Add(x => x.AddSingleton<ITRexConvertCoordinates>(new TRexConvertCoordinates()))
        .Add(VSS.TRex.IO.DIUtilities.AddPoolCachesToDI)
        .Add(VSS.TRex.Cells.DIUtilities.AddPoolCachesToDI)
        .Add(TRexGridFactory.AddGridFactoriesToDI)
        .Add(VSS.TRex.Storage.Utilities.DIUtilities.AddProxyCacheFactoriesToDI)
        .Build()
        .Add(x => x.AddSingleton<ISubGridCellSegmentPassesDataWrapperFactory>(new SubGridCellSegmentPassesDataWrapperFactory()))
        .Add(x => x.AddSingleton<ISubGridCellLatestPassesDataWrapperFactory>(new SubGridCellLatestPassesDataWrapperFactory()))
        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces.SurveyedSurfaces()))
        .Add(x => x.AddSingleton<ISurveyedSurfaceFactory>(new SurveyedSurfaces.SurveyedSurfaceFactory()))
        .Add(x => x.AddSingleton<ISubGridSpatialAffinityKeyFactory>(new SubGridSpatialAffinityKeyFactory()))
        .Build()
        .Add(x => x.AddServiceDiscovery())
        
        .Add(x => x.AddSingleton<ITagFileAuthProjectV5Proxy, TagFileAuthProjectV5Proxy>())
        .Add(x => x.AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>())
        .Add(x => x.AddTransient<ITPaasProxy, TPaasProxy>())
        .Add(x => x.AddSingleton<ICwsDeviceGatewayClient, CwsDeviceGatewayClient>())

        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels(StorageMutability.Mutable)))
        .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
        .Add(x => x.AddSingleton<IMutabilityConverter>(new MutabilityConverter()))
        .Add(TRex.ExistenceMaps.ExistenceMaps.AddExistenceMapFactoriesToDI)
        .Add(x => x.AddSingleton<IProductionEventsFactory>(new ProductionEventsFactory()))
        .Add(x => x.AddSingleton<ISegmentRetirementQueue>(factory => new SegmentRetirementQueue()))
        .Build()
        .Add(x => x.AddSingleton(new TagProcComputeServer()))
        .Add(x => x.AddTransient<IDesigns>(factory => new Designs.Storage.Designs()))
        .Add(x => x.AddSingleton<IDesignFiles>(new DesignFiles()))
        .Add(x => x.AddSingleton<IDesignManager>(factory => new DesignManager(StorageMutability.Mutable)))
        .Add(x => x.AddSingleton<ISurveyedSurfaceManager>(factory => new SurveyedSurfaceManager(StorageMutability.Mutable)))

        // Register the sender for the site model attribute change notifications
        .Add(x => x.AddSingleton<ISiteModelAttributesChangedEventSender>(new SiteModelAttributesChangedEventSender()))

        .Add(x => x.AddSingleton<ISiteModelMetadataManager>(factory => new SiteModelMetadataManager(StorageMutability.Mutable)))
        .Add(x => x.AddSingleton<ITRexHeartBeatLogger>(new TRexHeartBeatLogger()))

        .Add(x => x.AddTransient<IAlignments>(factory => new Alignments.Alignments()))
        .Add(x => x.AddSingleton<IAlignmentManager>(factory => new AlignmentManager(StorageMutability.Mutable)))

        .Add(x => x.AddSingleton<Func<ITAGFileBufferQueue>>(factory => () => new TAGFileBufferQueue()))
        .Add(x => x.AddMemoryCache())
        .Add(x => x.AddServiceDiscovery())
        .Add(x => x.AddSingleton<IDataCache>(factory => new InMemoryDataCache(DIContext.Obtain<ILoggerFactory>(), DIContext.Obtain<IMemoryCache>())))
        .Add(x => x.AddHttpClient())
        .Add(x => x.AddSingleton<IWebRequest>(factory => 
          new GracefulWebRequest(
            DIContext.Obtain<ILoggerFactory>(),
            DIContext.Obtain<IConfigurationStore>(),
            DIContext.Obtain<IHttpClientFactory>())))
        .Add(x => x.AddSingleton<IRebuildSiteModelTAGNotifier, RebuildSiteModelTAGNotifier>())
        .Add(x => x.AddSingleton<ITransferProxyFactory, TransferProxyFactory>())
        .Complete();
    }

    // This static array ensures that all required assemblies are included into the artifacts by the linker
    private static void EnsureAssemblyDependenciesAreLoaded()
    {
      // This static array ensures that all required assemblies are included into the artifacts by the linker
      Type[] AssemblyDependencies =
      {
        typeof(VSS.TRex.Geometry.BoundingIntegerExtent2D),
        typeof(VSS.TRex.GridFabric.BaseIgniteClass),
        typeof(VSS.TRex.Common.SubGridsPipelinedResponseBase),
        typeof(VSS.TRex.Logging.Logger),
        typeof(VSS.TRex.DI.DIContext),
        typeof(VSS.TRex.Storage.StorageProxy),
        typeof(VSS.TRex.SiteModels.SiteModel),
        typeof(VSS.TRex.Cells.CellEvents),
        typeof(VSS.TRex.Compression.AttributeValueModifiers),
        typeof(CoreXModels.LLH),
        typeof(VSS.TRex.Designs.DesignBase),
        typeof(VSS.TRex.Designs.TTM.HashOrdinate),
        typeof(VSS.TRex.Designs.TTM.Optimised.HeaderConsts),
        typeof(VSS.TRex.Events.CellPassFastEventLookerUpper),
        typeof(VSS.TRex.ExistenceMaps.ExistenceMaps),
        typeof(VSS.TRex.GridFabric.BaseIgniteClass),
        typeof(VSS.TRex.Machines.Machine),
        typeof(VSS.TRex.SubGridTrees.Client.ClientCMVLeafSubGrid),
        typeof(VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities),
        typeof(VSS.TRex.SubGridTrees.Server.MutabilityConverter),
        typeof(VSS.TRex.SurveyedSurfaces.SurveyedSurface),
        typeof(VSS.TRex.TAGFiles.Executors.SubmitTAGFileExecutor),
        typeof(CoreXModels.CoordinateSystemResponse),
        typeof(VSS.TRex.SiteModelChangeMaps.GridFabric.NodeFilters.SiteModelChangeProcessorRoleBasedNodeFilter)
      };

      foreach (var asmType in AssemblyDependencies)
      {
        if (asmType.FullName == "DummyTypeName")
          Console.WriteLine($"Assembly for type {asmType} has not been loaded.");
      }
    }

    /// <summary>
    /// Display environment variables
    ///   DependencyInjection() includes only IConfigurationStore (not IConfiguration)
    ///   Therefore the order here reflects IConfigurationStore so that matching
    ///     values will be used.
    /// </summary>
    private static void DisplayEnvironmentVariablesToConsole()
    {
      var configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
        .Build();

      Console.WriteLine("**** Configuration Settings ****");
      foreach (var env in configuration.GetChildren())
      {
        Console.WriteLine($"{env.Key}:{env.Value}");
      }
    }

    private static void DoServiceInitialisation()
    {
      // Register the heartbeat loggers
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new MemoryHeartBeatLogger());
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new RecycledMemoryStreamHeartBeatLogger());
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new SiteModelsHeartBeatLogger());
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new DotnetThreadHeartBeatLogger());
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new TAGFileProcessingHeartBeatLogger());
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new IgniteNodeMetricsHeartBeatLogger(DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Mutable)));

      IO.DIUtilities.AddHeartBeatLoggers();
    }

    static async Task<int> Main(string[] args)
    {
      try
      {
        Console.WriteLine($"TRex service starting at {DateTime.Now}");

        ThreadPool.GetMinThreads(out var minWorkerThreads, out var minCompletionPortThreads);
        ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);
        Console.WriteLine($"Thread pool: min threads {minWorkerThreads}/{minCompletionPortThreads}, max threads {maxWorkerThreads}/{maxCompletionPortThreads}");

        EnsureAssemblyDependenciesAreLoaded();
        DependencyInjection();

        DisplayEnvironmentVariablesToConsole();
        if (string.IsNullOrEmpty(DIContext.Obtain<IConfigurationStore>().GetValueString("ENABLE_TFA_SERVICE")))
          Console.WriteLine("*** Warning! **** Check for missing configuration values. e.g ENABLE_TFA_SERVICE");

        var cancelTokenSource = new CancellationTokenSource();
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
          Console.WriteLine("Exiting");
          DIContext.Obtain<ITRexGridFactory>().StopGrids();
          cancelTokenSource.Cancel();
        };

        AppDomain.CurrentDomain.UnhandledException += TRexAppDomainUnhandledExceptionHandler.Handler;

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
