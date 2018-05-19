using System;
using System.Windows.Forms;
using VSS.TRex.DI;

namespace TRexGridActivator
{
  static class Program
  {
    private static void DependencyInjection()
    {
      DIImplementation.New().ConfigureLogging().Complete();
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      DependencyInjection();

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Form1());
    }
  }
}
