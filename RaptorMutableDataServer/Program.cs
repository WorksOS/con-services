using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSS.VisionLink.Raptor;

namespace RaptorMutableDataServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string logFileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + string.Format("({0})", RaptorServerConfig.Instance().SpatialSubdivisionDescriptor) + ".log";
            log4net.GlobalContext.Properties["LogName"] = logFileName;
            log4net.Config.XmlConfigurator.Configure();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
