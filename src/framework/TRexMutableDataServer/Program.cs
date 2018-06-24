using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.CoordinateSystems.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.TAGFiles.Classes;

namespace TRexMutableDataServer
{
  static class Program
  {
    private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels()))
        .Add(x => x.AddSingleton<ITFAProxy>(new TFAProxy()))
        .Add(x => x.AddSingleton<ICoordinateConversion>(new CoordinateConversion()))
        .Complete();
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      DependencyInjection();

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Form1());
    }
  }
}
