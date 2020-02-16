using log4net;
using System;
using System.ServiceProcess;

namespace VSS.Nighthawk.NHBssSvc
{
    static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// The main entry point for the BSS Windows Service.
        /// </summary>
        static void Main()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure();
                Log.DebugFormat("Service starting..");
                Log.DebugFormat("{0}", Environment.MachineName.ToLower().Contains(BssSvcSettings.Default.AppServerContains.ToLower()));
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
        new NHBssService()
                };
                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("{0} \n {1} \n {2}", ex.Message, ex.InnerException, ex.StackTrace);
            }
        }
    }
}
