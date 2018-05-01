using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

#if NET_4_7
using Microsoft.AspNetCore.Hosting.WindowsServices;
using System.Diagnostics;
#endif


namespace VSS.Productivity3D.TagFileAuth.WebAPI
{
  /// <summary>
  /// Application entry point.
  /// </summary>
  public class Program
  {
    /// <summary>
    /// Default constructory.
    /// </summary>
    public static void Main(string[] args)
    {
      var config = new ConfigurationBuilder()
        .AddCommandLine(args)
        .Build();

#if NET_4_7
      //To run the service use https://docs.microsoft.com/en-us/aspnet/core/hosting/windows-service
      var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
      var pathToContentRoot = Path.GetDirectoryName(pathToExe);


      var host = new WebHostBuilder()
        .UseConfiguration(config)
        .UseKestrel()
        .UseContentRoot(pathToContentRoot)
        .UseIISIntegration()
        .UseStartup<Startup>();
      host.Build().RunAsService();
#else

      var host = new WebHostBuilder()
        .UseConfiguration(config)
        .UseKestrel()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        .UseStartup<Startup>();
      host.Build().Run();
#endif
    }
  }
}
