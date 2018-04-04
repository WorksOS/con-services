using System;
using System.Windows.Forms;
using VSS.Velociraptor.DesignProfiling;
using VSS.VisionLink.Raptor;

namespace VSS.Raptor.IgnitePOC.TestApp
{
    static class Program
    {
        static void DoTest()
        {
         TTMDesign design = new TTMDesign(SubGridTree.DefaultCellSize);
         design.LoadFromFile(@"C:\Temp\Bug36372.ttm");
        }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
        static void Main()
        {
            //DoTest();
            string logFileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".log";
            log4net.GlobalContext.Properties["LogName"] = logFileName;
            log4net.Config.XmlConfigurator.Configure();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
