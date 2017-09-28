using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.AspNetCore.Hosting.WindowsServices;

namespace VSS.Productivity3D.WebApi
{
  /// <summary>
  /// 
  /// </summary>
  public class Program
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
      bool isService = args.Contains("--service");

      var config = new ConfigurationBuilder()
        .AddCommandLine(args)
        .Build();

      //To run the service use https://docs.microsoft.com/en-us/aspnet/core/hosting/windows-service
      var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
      var pathToContentRoot = Path.GetDirectoryName(pathToExe);

      var host = new WebHostBuilder()
          .UseConfiguration(config)
          .UseKestrel()
          .UseContentRoot(pathToContentRoot)
          .UseIISIntegration()
          .UseStartup<Startup>()
          .Build();

if (!isService)
      host.Run();
else
      host.RunAsService();
    }
  }
}