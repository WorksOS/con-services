﻿using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.Servers.Client;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Services.Designs;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;

namespace TRexDesignElevationsServer
{
  static class Program
  {
    private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()))
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Build()
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels(() => DIContext.Obtain<IStorageProxyFactory>().ImmutableGridStorage())))
        .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
        .Add(x => x.AddSingleton<IExistenceMaps>(new ExistenceMaps()))
        .Add(x => x.AddSingleton(new CalculateDesignElevationsServer()))
        .Add(x => x.AddSingleton<IDesignsService>(new DesignsService(StorageMutability.Immutable)))
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
