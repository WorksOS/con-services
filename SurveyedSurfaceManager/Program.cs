using System;
using System.Windows.Forms;
using VSS.VisionLink.Raptor.Servers.Client;

namespace SurveyedSurfaceManager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string logFileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".log";
            log4net.GlobalContext.Properties["LogName"] = logFileName;
            log4net.Config.XmlConfigurator.Configure();
        
            ImmutableClientServer server = new ImmutableClientServer("SurveyedSurfaceManager");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
