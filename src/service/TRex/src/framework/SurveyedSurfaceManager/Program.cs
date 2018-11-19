using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using VSS.Log4Net.Extensions;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Servers.Client;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace SurveyedSurfaceManager
{
  static class Program
  {

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()))
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Add(x => x.AddSingleton<IExistenceMaps>(new ExistenceMaps()))
        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces()))
        .Add(x => x.AddSingleton<ISurveyedSurfaceFactory>(new SurveyedSurfaceFactory()))
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels(() => DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage())))
        .Build()
        .Add(x => x.AddSingleton(new MutableClientServer("SurveyedSurfaceManager")))
        .Add(x => x.AddSingleton<IDesignManager>(factory => new DesignManager()))
        .Add(x => x.AddSingleton<ISurveyedSurfaceManager>(factory => new VSS.TRex.SurveyedSurfaces.SurveyedSurfaceManager()))

        .Complete();
    }

    [STAThread]
    static void Main()
    {
      Log4NetAspExtensions.ConfigureLog4Net("TRex");
      DependencyInjection();

      // Make sure all our assemblies are loaded...
      AssembliesHelper.LoadAllAssembliesForExecutingContext();

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Form1());
    }
  }
}
