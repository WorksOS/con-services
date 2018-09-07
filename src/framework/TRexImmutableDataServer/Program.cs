using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Common.Utilities;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.ExistenceMaps;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.Factories;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Services.Designs;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace TRexImmutableDataServer
{
  static class Program
  {
    private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()

        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces()))
        .Add(x => x.AddSingleton<ISurveyedSurfaceFactory>(new SurveyedSurfaceFactory()))
        .Build()
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels(DIContext.Obtain<IStorageProxyFactory>().ImmutableGridStorage())))
        .Add(x => x.AddSingleton<IProfilerBuilderFactory>(new ProfilerBuilderFactory()))
        .Add(x => x.AddTransient<IProfilerBuilder>(factory => new ProfilerBuilder()))
        .Add(x => x.AddSingleton<IDesignsService>(new DesignsService(StorageMutability.Immutable)))
        .Add(x => x.AddSingleton<IExistenceMaps>(new ExistenceMaps()))
        .Add(x => x.AddSingleton<IProductionEventsFactory>(factory => new ProductionEventsFactory()))

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
