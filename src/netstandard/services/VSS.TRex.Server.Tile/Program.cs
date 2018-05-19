using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Implementations.Core2;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.Server.Application
{
  class Program
  {
    private static void DependencyInjection()
    {
      DIContext.Inject(
        DIImplementation.New()
        .ConfigureLogging()
        .Configure(collection =>
        {
          // The renderer factory that allows tile rendering services access Bitmap etc platform dependent constructs
          collection.AddSingleton<IRenderingFactory>(new RenderingFactory());
        }).Build());
    }

//    private static void DependencyInjection()
//    {
//      DIContext.Inject(DIImplementation.New().ConfigureLogging().Build());
//    }


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
