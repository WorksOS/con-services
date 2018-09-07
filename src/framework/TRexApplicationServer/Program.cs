using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Common.Utilities;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.CoordinateSystems.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Exports.Patches.Executors.Tasks;
using VSS.TRex.Exports.Surfaces.Executors.Tasks;
using VSS.TRex.GridFabric.Models.Arguments;
using VSS.TRex.GridFabric.Models.Responses;
using VSS.TRex.Pipelines;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Executors.Tasks;
using VSS.TRex.Rendering.Implementations.Framework;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace TRexApplicationServer
{
  static class Program
  {
    private static ISubGridPipelineBase SubGridPipelineFactoryMethod(PipelineProcessorPipelineStyle key)
    {
      switch (key)
      {
        case PipelineProcessorPipelineStyle.DefaultAggregative:
          return new SubGridPipelineAggregative<SubGridsRequestArgument, SubGridRequestsResponse>();
        case PipelineProcessorPipelineStyle.DefaultProgressive:
          return new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>();
        default:
          return null;
      }
    }

    private static ITask SubGridTaskFactoryMethod(PipelineProcessorTaskStyle key)
    {
      switch (key)
      {
        case PipelineProcessorTaskStyle.AggregatedPipelined:
          return new AggregatedPipelinedSubGridTask();
        case PipelineProcessorTaskStyle.PVMRendering:
          return new PVMRenderingTask();
        case PipelineProcessorTaskStyle.PatchExport:
          return new PatchTask();
        case PipelineProcessorTaskStyle.SurfaceExport:
          return new SurfaceTask();
        default:
          return null;
      }
    }

    private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces()))
        .Add(x => x.AddSingleton<ISurveyedSurfaceFactory>(new SurveyedSurfaceFactory()))
        .Build()

        // The renderer factory that allows tile rendering services access Bitmap etc platform dependent constructs
        .Add(x => x.AddSingleton<IRenderingFactory>(new RenderingFactory()))

        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels(DIContext.Obtain<IStorageProxyFactory>().ImmutableGridStorage())))
        .Add(x => x.AddSingleton<ICoordinateConversion>(new CoordinateConversion()))
        .Add(x => x.AddSingleton<IExistenceMaps>(new VSS.TRex.ExistenceMaps.ExistenceMaps()))
        .Add(x => x.AddSingleton<IPipelineProcessorFactory>(new PipelineProcessorFactory()))
        .Add(x => x.AddSingleton<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>(provider => SubGridPipelineFactoryMethod))
        .Add(x => x.AddTransient<IRequestAnalyser>(factory => new RequestAnalyser()))
        .Add(x => x.AddSingleton<Func<PipelineProcessorTaskStyle, ITask>>(provider => SubGridTaskFactoryMethod))

        .Complete();
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      DependencyInjection();

      // Make sure all our assemblies are loaded...
      AssembliesHelper.LoadAllAssembliesForExecutingContext();

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Form1());
    }
  }
}
