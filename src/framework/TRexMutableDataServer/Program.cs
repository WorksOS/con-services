using System;
using System.Windows.Forms;

namespace TRexMutableDataServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string logFileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + $"-({DateTime.Now.Ticks}).log";
            log4net.GlobalContext.Properties["LogName"] = logFileName;
            log4net.Config.XmlConfigurator.Configure();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
