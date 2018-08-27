using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Common.Utilities;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Servers.Client;
using VSS.TRex.Services.Designs;
using VSS.TRex.Storage.Models;
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
        .Add(x => x.AddSingleton<IDesignsService>(new DesignsService(StorageMutability.Immutable)))
        .Add(x => x.AddSingleton<IExistenceMaps>(new ExistenceMaps()))
        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces()))
        .Add(x => x.AddSingleton<ISurveyedSurfaceFactory>(new SurveyedSurfaceFactory()))
        .Complete();
    }

    [STAThread]
    static void Main()
    {
      DependencyInjection();

      // Make sure all our assemblies are loaded...
      AssembliesHelper.LoadAllAssembliesForExecutingContext();

      ImmutableClientServer server = new ImmutableClientServer("SurveyedSurfaceManager");

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Form1());
    }
  }
}
