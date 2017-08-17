using System.IO;
using Microsoft.AspNetCore.Hosting;

#if NET_4_7
using Microsoft.AspNetCore.Hosting.WindowsServices;
using System.Diagnostics;
#endif

namespace VSS.Productivity3D.Scheduler.WebApi
{
  /// <summary>
  /// VSS.Productivity3D.Scheduler program
  /// </summary>
  public class Program
  {
    /// <summary>
    /// Hangfire scheduler background job runs at this address
    /// </summary>
    private const string endpoint = "http://localhost:12346"; // todo how to determine unique ports in 3dp system e.g. MockProjectWebApi is 5001; FilterSvc = ?; 3dp=?; etc

    /// <summary>
    /// VSS.Productivity3D.Scheduler main
    /// </summary>
    public static void Main(string[] args)
    {
     
#if NET_4_7 //To run the service use https://docs.microsoft.com/en-us/aspnet/core/hosting/windows-service
      var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
      var pathToContentRoot = Path.GetDirectoryName(pathToExe);


      var host = new WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(pathToContentRoot)
        .UseIISIntegration()
        .UseStartup<Startup>()
        .UseUrls(endpoint)
        .Build();

      host.RunAsService();
#else
      var host = new WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        .UseStartup<Startup>() 
        .UseUrls(endpoint)
        .Build();

      host.Run();
#endif
    }
  }
}