using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using VSS.VisionLink.Raptor.DI;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Implementations.Core2;
using VSS.VisionLink.Raptor.Servers.Client;

namespace VSS.TRex.Server.Application
{
  class Program
  {
    private static ILog Log;
    private static void CreateDependencyInjection()
    {
      DIImplementation DI = new DIImplementation(
          collection =>
          {
                  // Inject the renderer factory that allows tile rendering services access Bitmap etc pltform depenendent constructs
                  collection.AddSingleton<IRenderingFactory>(new RenderingFactory());

                  // Microsoft.Dependencies.Logging related DI. Currently Trec uses Log4net...
                  // Make a logger factory for when a new logger is required
                  //      ILoggerFactory loggerFactory = new  LoggerFactory();
                  //      collection.AddSingleton<ILoggerFactory>(loggerFactory);

                  // Make a default common logger instance for then that is enough
                  //     collection.AddSingleton<ILogger>(new Logger<Program>(loggerFactory));
                });

      DIContext.Inject(DI.ServiceProvider);
    }

    static void Main(string[] args)
    {
      // Initialise the Log4Net logging system
      var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
      string s = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "log4net.xml");
      XmlConfigurator.Configure(logRepository, new FileInfo(s));
      Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

      CreateDependencyInjection();

      var server = new ApplicationServiceServer();
      Console.WriteLine("Press anykey to exit");
      Console.ReadLine();
    }
  }
}
