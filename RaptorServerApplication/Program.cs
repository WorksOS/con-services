using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using RaptorTileServer;
using VSS.TRex.DI;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Implementations.Framework;

namespace TRexServerApplication
{
    static class Program
    {
        private static void CreateDependencyInjection()
        {
            DIImplementation DI = new DIImplementation(
                collection =>
                {
                    // Inject the renderer factory that allows tile rendering services access Bitmap etc pltform depenendent constructs
                    collection.AddSingleton<IRenderingFactory>(new RenderingFactory());

//                    collection
//                        .AddScoped<VSS.TRex.Rendering.Implementations.Framework.GridFabric.Responses.
//                            TileRenderResponse_Framework>();

                    // Microsoft.Dependencies.Logging related DI. Currently Trec uses Log4net...
                    // Make a logger factory for when a new logger is required
                    //      ILoggerFactory loggerFactory = new  LoggerFactory();
                    //      collection.AddSingleton<ILoggerFactory>(loggerFactory);

                    // Make a default common logger instance for then that is enough
                    //     collection.AddSingleton<ILogger>(new Logger<Program>(loggerFactory));
                });

            DIContext.Inject(DI.ServiceProvider);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string logFileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".log";
            log4net.GlobalContext.Properties["LogName"] = logFileName;
            log4net.Config.XmlConfigurator.Configure();

            CreateDependencyInjection();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
