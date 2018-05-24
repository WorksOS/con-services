using System;
using System.Windows.Forms;
using log4net;

namespace TagFileUtility
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      LogManager.GetLogger(typeof(Program));
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new UndoCheckout());
    }
  }
}
