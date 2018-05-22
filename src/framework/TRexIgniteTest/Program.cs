﻿using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.IgnitePOC.TestApp
{
  static class Program
  {
    private static void DependencyInjection()
    {
      DIBuilder.New().AddLogging().Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory())).Complete();
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
