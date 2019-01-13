using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.TRex.HttpClients.Clients;
using VSS.TRex.HttpClients.RequestHandlers;
using VSS.TRex.Common;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Executors.Tasks;
using VSS.TRex.Rendering.Implementations.Core2;
using VSS.TRex.Rendering.Servers.Client;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.Server.TileRendering
{
  class Program
  {
    private static ISubGridPipelineBase SubGridPipelineFactoryMethod(PipelineProcessorPipelineStyle key)
    {
      switch (key)
      {
        case PipelineProcessorPipelineStyle.DefaultProgressive:
          return new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>();
        default:
          return null;
      }
    }

    private static Pipelines.Interfaces.Tasks.ITRexTask SubGridTaskFactoryMethod(PipelineProcessorTaskStyle key)
    {
      switch (key)
      {
        case PipelineProcessorTaskStyle.PVMRendering:
          return new PVMRenderingTask();
        default:
          return null;
      }
    }

    private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()))
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces.SurveyedSurfaces()))
        .Add(x => x.AddSingleton<ISurveyedSurfaceFactory>(new SurveyedSurfaceFactory()))
        .Build()
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels(() => DIContext.Obtain<IStorageProxyFactory>().ImmutableGridStorage())))
        .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))

        // The renderer factory that allows tile rendering services access Bitmap etc platform dependent constructs
        .Add(x => x.AddSingleton<IRenderingFactory>(new RenderingFactory()))

        .Add(x => x.AddSingleton<IExistenceMaps>(new ExistenceMaps.ExistenceMaps()))
        .Add(x => x.AddSingleton<IPipelineProcessorFactory>(new PipelineProcessorFactory()))
        .Add(x => x.AddSingleton<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>(provider => SubGridPipelineFactoryMethod))
        .Add(x => x.AddTransient<IRequestAnalyser>(factory => new RequestAnalyser()))
        .Add(x => x.AddSingleton<Func<PipelineProcessorTaskStyle, ITRexTask>>(provider => SubGridTaskFactoryMethod))
        .Add(x => x.AddSingleton<IClientLeafSubGridFactory>(ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory()))
        .Build()
        .Add(x => x.AddSingleton(new TileRenderingServer()))
        .Add(x => x.AddSingleton<IDesignManager>(factory => new DesignManager()))
        .Add(x => x.AddSingleton<ISurveyedSurfaceManager>(factory => new SurveyedSurfaceManager()))

        // Register the listener for site model attribute change notifications
        .Add(x => x.AddSingleton<ISiteModelAttributesChangedEventListener>(new SiteModelAttributesChangedEventListener(TRexGrids.ImmutableGridName())))

        .Add(x => x.AddSingleton<ITRexHeartBeatLogger>(new TRexHeartBeatLogger()))

        // Setup dependencies for communication with the Trimble Coordinate Service.
        .Add(x => x.AddSingleton<ITPaasProxy, TPaasProxy>())
        .Add(x => x.AddTransient<TPaaSAuthenticatedRequestHandler>())
        .Add(x => x.AddTransient<TPaaSApplicationCredentialsRequestHandler>())
        .AddHttpClient<TPaaSClient>(client => client.BaseAddress = new Uri("https://identity-stg.trimble.com/i/oauth2/token"))
          .AddHttpMessageHandler<TPaaSApplicationCredentialsRequestHandler>()
        .AddHttpClient<CoordinatesServiceClient>(client => client.BaseAddress = new Uri("https://api-stg.trimble.com/t/trimble.com/coordinates/1.0"))
          .AddHttpMessageHandler<TPaaSAuthenticatedRequestHandler>()
        .Add(x => x.AddTransient<IFilterSet>(factory => new FilterSet()))
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
        typeof(VSS.TRex.CoordinateSystems.Models.LLH),
        typeof(VSS.TRex.ExistenceMaps.ExistenceMaps),
        typeof(VSS.TRex.Filters.CellPassAttributeFilter),
        typeof(VSS.TRex.GridFabric.BaseIgniteClass),
        typeof(VSS.TRex.Machines.Machine),
        typeof(VSS.TRex.Pipelines.PipelineProcessor),
        typeof(VSS.TRex.Rendering.PlanViewTileRenderer),
        typeof(VSS.TRex.Services.Designs.DesignsService),
        typeof(VSS.TRex.Services.SurveyedSurfaces.SurveyedSurfaceService),
        typeof(VSS.TRex.SubGrids.CutFillUtilities),
        typeof(VSS.TRex.SubGridTrees.Client.ClientCMVLeafSubGrid),
        typeof(VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities),
        typeof(VSS.TRex.SurveyedSurfaces.SurveyedSurface),
        typeof(VSS.TRex.Rendering.Implementations.Core2.RenderingFactory),
        typeof(VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses.TileRenderResponse_Core2),
      };

      foreach (var asmType in AssemblyDependencies)
        if (asmType.Assembly == null)
          Console.WriteLine($"Assembly for type {asmType} has not been loaded.");
    }

    private static void DoServiceInitialisation()
    {
      // Start listening to site model change notifications
      DIContext.Obtain<ISiteModelAttributesChangedEventListener>().StartListening();

      // Register the heartbeat loggers
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new MemoryHeartBeatLogger());
    }

    static async Task<int> Main(string[] args)
    {
      EnsureAssemblyDependenciesAreLoaded();
      DependencyInjection();

      var cancelTokenSource = new CancellationTokenSource();
      AppDomain.CurrentDomain.ProcessExit += (s, e) =>
      {
        Console.WriteLine("Exiting");
        cancelTokenSource.Cancel();
      };

      DoServiceInitialisation();

      await Task.Delay(-1, cancelTokenSource.Token);
      return 0;
    }
  }
}
