using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Implementations.Core2;
using VSS.TRex.Servers.Client;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Server.Application
{
  class Program
  {
    private static void DependencyInjection()
    {
        DIImplementation.New()
        .AddLogging()
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Add(collection =>
        {
          // The renderer factory that allows tile rendering services access Bitmap etc platform dependent constructs
          collection.AddSingleton<IRenderingFactory>(new RenderingFactory());
        }).Complete();
    }

    static void Main(string[] args)
    {
      DependencyInjection();

      ILogger Log = Logging.Logger.CreateLogger<Program>();

      Log.LogInformation("Creating service");
      Log.LogDebug("Creating service");

      var server = new ApplicationServiceServer();
      Console.WriteLine("Press anykey to exit");
      Console.ReadLine();
    }
  }
}
