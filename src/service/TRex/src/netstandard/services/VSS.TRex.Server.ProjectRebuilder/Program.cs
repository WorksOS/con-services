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
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Executors;
using VSS.TRex.SiteModels.GridFabric.Listeners;
using VSS.TRex.SiteModels.Heartbeats;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.SiteModels.Interfaces.Listeners;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;

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

        //        .Build()
        //        .Add(x => x.AddSingleton<IConvertCoordinates>(new ConvertCoordinates()))
        //        .Add(VSS.TRex.IO.DIUtilities.AddPoolCachesToDI)
        //        .Add(VSS.TRex.Cells.DIUtilities.AddPoolCachesToDI)
        .Add(TRexGridFactory.AddGridFactoriesToDI)
        //        .Add(VSS.TRex.Storage.Utilities.DIUtilities.AddProxyCacheFactoriesToDI)
        //        .Build()
        //        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces.SurveyedSurfaces()))
        //        .Add(x => x.AddSingleton<ISurveyedSurfaceFactory>(new SurveyedSurfaceFactory()))
        //        .Build()
        //        .Add(x => x.AddTransient<IFilterSet>(factory => new FilterSet()))
        //        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels()))
        //        .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
        //        .Add(ExistenceMaps.ExistenceMaps.AddExistenceMapFactoriesToDI)
        //        .Add(x => x.AddSingleton<IPipelineProcessorFactory>(new PipelineProcessorFactory()))
        //        .Add(x => x.AddSingleton<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>(provider => SubGridPipelineFactoryMethod))
        //        .Add(x => x.AddTransient<IRequestAnalyser>(factory => new RequestAnalyser()))
        //        .Add(x => x.AddSingleton<Func<PipelineProcessorTaskStyle, ITRexTask>>(provider => SubGridTaskFactoryMethod))
        //        .Add(x => x.AddSingleton<IClientLeafSubGridFactory>(ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory()))
        .Add(x => x.AddSingleton<ITRexHeartBeatLogger, TRexHeartBeatLogger>())
        //
        //        // Register the listener for site model attribute change notifications
        //        .Add(x => x.AddSingleton<ISiteModelAttributesChangedEventListener>(new SiteModelAttributesChangedEventListener(TRexGrids.ImmutableGridName())))
        //
        //        // Register the factory for the CellProfileAnalyzer for detailed call pass/lift cell profiles
        //        .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, ICellLiftBuilder, IOverrideParameters, ILiftParameters, ICellProfileAnalyzer<ProfileCell>>>(
        //          factory => (siteModel, pDExistenceMap, filterSet, cellLiftBuilder, overrides, liftParams)
        //            => new CellProfileAnalyzer(siteModel, pDExistenceMap, filterSet, cellLiftBuilder, overrides, liftParams)))
        //
        //        // Register the factory for the CellProfileAnalyzer for summary volume cell profiles
        //        .Add(x => x.AddTransient<Func<ISiteModel, ISubGridTreeBitMask, IFilterSet, ICellLiftBuilder, ICellProfileAnalyzer<SummaryVolumeProfileCell>>>(
        //          factory => (siteModel, pDExistenceMap, filterSet, cellLiftBuilder) => null))
        //
        //        .Add(x => x.AddSingleton<IPipelineListenerMapper>(new PipelineListenerMapper()))

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
        /* Copied from application service
         * 
        typeof(VSS.TRex.Analytics.MDPStatistics.MDPStatisticsAggregator),
        typeof(VSS.TRex.Geometry.BoundingIntegerExtent2D),
        typeof(VSS.TRex.Exports.Patches.PatchResult),
        typeof(VSS.TRex.GridFabric.BaseIgniteClass),
        typeof(VSS.TRex.Common.SubGridsPipelinedResponseBase),
        */
        typeof(VSS.TRex.Logging.Logger)
        /*
        typeof(VSS.TRex.DI.DIContext),
        typeof(VSS.TRex.Storage.StorageProxy),
        typeof(VSS.TRex.SiteModels.SiteModel),
        typeof(VSS.TRex.Cells.CellEvents),
        typeof(VSS.TRex.Compression.AttributeValueModifiers),
        typeof(VSS.TRex.CoordinateSystems.Models.LLH),
        typeof(VSS.TRex.Designs.DesignBase),
        typeof(VSS.TRex.Designs.TTM.HashOrdinate),
        typeof(VSS.TRex.Designs.TTM.Optimised.HeaderConsts),
        typeof(VSS.TRex.Events.CellPassFastEventLookerUpper),
        typeof(VSS.TRex.ExistenceMaps.ExistenceMaps),
        typeof(VSS.TRex.Filters.CellPassAttributeFilter),
        typeof(VSS.TRex.GridFabric.BaseIgniteClass),
        typeof(VSS.TRex.Machines.Machine),
        typeof(VSS.TRex.Pipelines.PipelineProcessor<SubGridsRequestArgument>),
        typeof(VSS.TRex.Profiling.CellLiftBuilder),
        typeof(VSS.TRex.SubGrids.CutFillUtilities),
        typeof(VSS.TRex.SubGridTrees.Client.ClientCMVLeafSubGrid),
        typeof(VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities),
        typeof(VSS.TRex.SubGridTrees.Server.MutabilityConverter),
        typeof(VSS.TRex.SurveyedSurfaces.SurveyedSurface),
        typeof(VSS.TRex.Volumes.CutFillVolume),
        typeof(VSS.TRex.CellDatum.GridFabric.Responses.CellDatumResponse_ApplicationService),
        typeof(VSS.TRex.SiteModelChangeMaps.GridFabric.Services.SiteModelChangeProcessorService)
        */
      };

      foreach (var asmType in AssemblyDependencies)
      {
        if (asmType.FullName == "DummyTypeName")
          Console.WriteLine($"Assembly for type {asmType} has not been loaded.");
      }
    }

    private static void DoServiceInitialisation()
    {
      // Register the heartbeat loggers
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new MemoryHeartBeatLogger());
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new IgniteNodeMetricsHeartBeatLogger(DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Mutable)));
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new SiteModelRebuilderHeartbeatLogger());
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
