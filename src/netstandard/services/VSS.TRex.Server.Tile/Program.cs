using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.CoordinateSystems.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Implementations.Core2;
using VSS.TRex.Servers.Client;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Server.Application
{
  class Program
  {
    //Event to signal when to shutdown
    private static readonly AutoResetEvent WaitHandle = new AutoResetEvent(false);

    private static void DependencyInjection()
    {
        DIBuilder.New()
        .AddLogging()
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels()))

        // The renderer factory that allows tile rendering services access Bitmap etc platform dependent constructs
        .Add(x => x.AddSingleton<IRenderingFactory>(new RenderingFactory()))

        .Add(x => x.AddSingleton<ICoordinateConversion>(new CoordinateConversion()))
        .Complete();
    }

    static void Main(string[] args)
    {
      DependencyInjection();

      ILogger Log = Logging.Logger.CreateLogger<Program>();

      Log.LogInformation("Creating service");
      Log.LogDebug("Creating service");

      var server = new ApplicationServiceServer();
      Console.WriteLine("Press ctrl+c to exit");
      Console.CancelKeyPress += (s, a) => 
        { Console.WriteLine("Exiting");
          WaitHandle.Set();
        };

      WaitHandle.WaitOne();
    }
  }
}
