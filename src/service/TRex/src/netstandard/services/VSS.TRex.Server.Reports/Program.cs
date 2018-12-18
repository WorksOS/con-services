using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.TRex.Common;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Reports.Servers.Client;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Reports.Gridded;
using VSS.TRex.Reports.Gridded.Executors.Tasks;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.Server.Reports
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

    private static ITRexTask SubGridTaskFactoryMethod(PipelineProcessorTaskStyle key)
    {
      switch (key)
      {
        case PipelineProcessorTaskStyle.GriddedReport:
          return new GriddedReportTask();
        default:
          return null;
      }
    }

    private static void DependencyInjection()
    {
      DIBuilder.New()
      .AddLogging()
      .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
      .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()))
      .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
      .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces.SurveyedSurfaces()))
      .Add(x => x.AddSingleton<ISurveyedSurfaceFactory>(new SurveyedSurfaceFactory()))
      .Build()
      .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels(() => DIContext.Obtain<IStorageProxyFactory>().ImmutableGridStorage())))
      .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
      .Add(x => x.AddSingleton<IExistenceMaps>(new ExistenceMaps.ExistenceMaps()))
      .Add(x => x.AddSingleton<IPipelineProcessorFactory>(new PipelineProcessorFactory()))
      .Add(x => x.AddSingleton<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>(provider => SubGridPipelineFactoryMethod))
      .Add(x => x.AddTransient<IRequestAnalyser>(factory => new RequestAnalyser()))
      .Add(x => x.AddSingleton<Func<PipelineProcessorTaskStyle, ITRexTask>>(provider => SubGridTaskFactoryMethod))
      .Add(x => x.AddSingleton<IClientLeafSubgridFactory>(ClientLeafSubgridFactoryFactory.CreateClientSubGridFactory()))
      .Build()
      .Add(x => x.AddSingleton(new GriddedReportRequestServer()))
      .Add(x => x.AddSingleton<IDesignManager>(factory => new DesignManager()))
      .Add(x => x.AddSingleton<ISurveyedSurfaceManager>(factory => new SurveyedSurfaceManager()))
      .Add(x => x.AddTransient<IFilterSet>(factory => new FilterSet()))

      .Add(x => x.AddSingleton<ITRexHeartBeatLogger>(new TRexHeartBeatLogger()))

      .Complete();
    }

    private static void EnsureAssemblyDependenciesAreLoaded()
    {
      // This static array ensures that all required assemblies are included into the artifacts by the linker
      Type[] AssemblyDependencies =
      {
        typeof(Geometry.BoundingIntegerExtent2D),
        typeof(GriddedReportResult),
        typeof(GridFabric.BaseIgniteClass),
        typeof(SubGridsPipelinedResponseBase),
        typeof(Logging.Logger),
        typeof(DIContext),
        typeof(StorageProxy),
        typeof(SiteModel),
        typeof(Cells.CellEvents),
        typeof(Compression.AttributeValueModifiers),
        typeof(CoordinateSystems.Models.LLH),
        typeof(DesignBase),
        typeof(Designs.TTM.HashOrdinate),
        typeof(Designs.TTM.Optimised.HeaderConsts),
        typeof(Events.CellPassFastEventLookerUpper),
        typeof(ExistenceMaps.ExistenceMaps),
        typeof(CellPassAttributeFilter),
        typeof(GridFabric.BaseIgniteClass),
        typeof(Machines.Machine),
        typeof(PipelineProcessor),
        typeof(Profiling.CellLiftBuilder),
        typeof(Rendering.PlanViewTileRenderer),
        typeof(Services.Designs.DesignsService),
        typeof(Services.SurveyedSurfaces.SurveyedSurfaceService),
        typeof(SubGrids.CutFillUtilities),
        typeof(ClientCMVLeafSubGrid),
        typeof(SubGridTrees.Core.Utilities.SubGridUtilities),
        typeof(SubGridTrees.Server.MutabilityConverter),
        typeof(SurveyedSurface)
      };

      foreach (var asmType in AssemblyDependencies)
        if (asmType.Assembly == null)
          Console.WriteLine($"Assembly for type {asmType} has not been loaded.");
    }

    private static void DoServiceInitialisation()
    {
      // Register the heartbeat loggers
      DIContext.Obtain<ITRexHeartBeatLogger>().AddContext(new MemoryHeartBeatLogger());
    }

    static async Task<int> Main(string[] args)
    {
      EnsureAssemblyDependenciesAreLoaded();
      DependencyInjection();

      ILogger Log = Logging.Logger.CreateLogger<Program>();

      Log.LogInformation("Creating service");
      Log.LogDebug("Creating service");

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
