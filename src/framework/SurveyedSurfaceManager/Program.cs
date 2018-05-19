using System;
using System.Windows.Forms;
using VSS.TRex.DI;
using VSS.TRex.Servers.Client;

namespace SurveyedSurfaceManager
{
  static class Program
  {

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    private static void DependencyInjection()
    {
      DIContext.Inject(DIImplementation.New().ConfigureLogging().Build());
    }

    [STAThread]
    static void Main()
    {
      DependencyInjection();
      ImmutableClientServer server = new ImmutableClientServer("SurveyedSurfaceManager");

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Form1());
    }
  }
}
