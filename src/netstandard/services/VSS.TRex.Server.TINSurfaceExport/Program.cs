using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Exports.Servers.Client;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Server.TINSurfaceExport
{
  class Program
  {
    private static void DependencyInjection()
    {
        DIBuilder.New()
        .AddLogging()
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels()))
        .Complete();
    }

    static void Main(string[] args)
    {
      DependencyInjection();

      ILogger Log = Logging.Logger.CreateLogger<Program>();

      Log.LogInformation("Creating service");
      Log.LogDebug("Creating service");

      var server = new TINSurfaceExportRequestServer();
      Console.WriteLine("Press anykey to exit");
      Console.ReadLine();
    }
  }
}
