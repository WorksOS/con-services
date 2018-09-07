using System;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Common.Utilities;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.CoordinateSystems.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.ExistenceMaps;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.TAGFiles.Classes;

namespace TRexMutableDataServer
{
  static class Program
  {

    static public IConfiguration Configuration { get; set; }

    private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces()))
        .Add(x => x.AddSingleton<ISurveyedSurfaceFactory>(new SurveyedSurfaceFactory()))
        .Build()
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels(DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage())))
        .Add(x => x.AddTransient<ISiteModel>(factory => new SiteModel()))
        .Add(x => x.AddSingleton<ITFAProxy>(new TFAProxy(Configuration)))
        .Add(x => x.AddSingleton<ICoordinateConversion>(new CoordinateConversion()))
        .Add(x => x.AddSingleton(Configuration))
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
      Configuration = new ConfigurationBuilder()
        .Build();

      DependencyInjection();

      // Make sure all our assemblies are loaded...
      AssembliesHelper.LoadAllAssembliesForExecutingContext();

      if (Configuration.GetSection("EnableTFAService").Value == null)
        Console.WriteLine("*** Warning! **** Check for missing configuration values. e.g EnableTFAService");

      Console.WriteLine("**** Configuration Settings ****");
      foreach (var env in Configuration.GetChildren())
      {
        Console.WriteLine($"{env.Key}:{env.Value}");
      }

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Form1());
    }
  }
}
