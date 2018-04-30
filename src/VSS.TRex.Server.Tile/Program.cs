using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using VSS.VisionLink.Raptor.DI;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Implementations.Core2;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.Servers.Client;

namespace VSS.TRex.Server.Application
{
    class Program
    {
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
            string logFileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".log";
            log4net.GlobalContext.Properties["LogName"] = logFileName;
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository);

            CreateDependencyInjection();

            var server = new RaptorApplicationServiceServer();
//            Console.WriteLine($"Spatial Division {RaptorServerConfig.Instance().SpatialSubdivisionDescriptor}");
            Console.WriteLine("Press anykey to exit");
            Console.ReadLine();
        }
    }
}
