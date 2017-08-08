using System.IO;
using Microsoft.AspNetCore.Hosting;

#if NET_4_7
using Microsoft.AspNetCore.Hosting.WindowsServices;
using System.Diagnostics;
#endif

namespace VSS.Productivity3D.Filter.WebApi
{
  /// <summary>
  /// VSS.Productivity3D.Filter program
  /// </summary>
  public class Program
  {
    /// <summary>
    /// VSS.Productivity3D.Filter main
    /// </summary>
    public static void Main(string[] args)
    {
      var host = new WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        .UseStartup<Startup>()
        .Build();

      host.Run();
    }
  }
}
