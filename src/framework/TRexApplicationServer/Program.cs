using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Common.Utilities;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.CoordinateSystems.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Rendering.Abstractions;
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

        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels()))
        .Add(x => x.AddSingleton<ICoordinateConversion>(new CoordinateConversion()))
        .Add(x => x.AddSingleton<IExistenceMaps>(new VSS.TRex.ExistenceMaps.ExistenceMaps()))

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
