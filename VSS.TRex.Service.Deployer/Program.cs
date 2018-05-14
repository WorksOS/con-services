using System;
using System.Reflection;
using log4net;
using log4net.Config;
using VSS.TRex.TAGFiles.GridFabric.Services;
using VSS.VisionLink.Raptor.Servers.Client;

namespace TRex.Service.Deployer
{
    /// <summary>
    /// This command line tool deploys all grid deployed services into the appropriate mutable and immutable
    /// data grids in TRex.
    /// </summary>
    class Program
    {
        private static ILog Log;

        static void Main(string[] args)
        {
            // Initialise the Log4Net logging system
            string logFileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".log";
            log4net.GlobalContext.Properties["LogName"] = logFileName;
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository);

            Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            
            // Active local client Ignite node
            RaptorMutableClientServer deployServer = new RaptorMutableClientServer("ServiceDeployer");

            Log.Info($"Obtaining proxy for TAG file buffer queue service");
            // Ensure the continuous query service is installed that supports TAG file processing
            TAGFileBufferQueueServiceProxy proxy = new TAGFileBufferQueueServiceProxy();
            try
            {
                Log.Info($"Deploying TAG file buffer queue service");
                proxy.Deploy();
            }
            catch (Exception e)
            {
                Log.Error($"Exception occurred deploying service: {e}");
            }
            Log.Info($"Complected service deployment for TAG file buffer queue service");
        }
    }
}
