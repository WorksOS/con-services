using System;
using System.Reflection;
using log4net;
using log4net.Config;
using VSS.Velociraptor.DesignProfiling.Servers.Client;

namespace VSS.TRex.Server.DesignElevation
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

            var server = new CalculateDesignElevationsServer();
            Console.WriteLine("Press anykey to exit");
            Console.ReadLine();
        }
    }
}
