using System;
using System.Reflection;
using log4net;
using log4net.Config;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.Servers.Compute;

namespace VSS.TRex.Server.PSNode
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialise the Log4Net logging system
            string logFileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".log";
            log4net.GlobalContext.Properties["LogName"] = logFileName;
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository);

            var server = new RaptorSubGridProcessingServer();
            Console.WriteLine($"Spatial Division {RaptorServerConfig.Instance().SpatialSubdivisionDescriptor}");
            Console.WriteLine("Press anykey to exit");
            Console.ReadLine();
        }
    }
}
