﻿using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.Profiling.Factories;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;

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
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels()))
        .Add(x => x.AddSingleton<IProfilerBuilderFactory>(new ProfilerBuilderFactory()))
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
