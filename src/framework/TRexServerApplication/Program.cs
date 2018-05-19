using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Implementations.Framework;

namespace TRexServerApplication
{
  static class Program
  {
    private static void DependencyInjection()
    {
      DIContext.Inject(
        DIImplementation.New()
          .ConfigureLogging()
          .Configure(collection =>
          {
            // The renderer factory that allows tile rendering services access Bitmap etc platform dependent constructs
            collection.AddSingleton<IRenderingFactory>(new RenderingFactory());
          }).Build());
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
