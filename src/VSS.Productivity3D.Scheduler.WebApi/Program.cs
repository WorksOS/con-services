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
    private const string Endpoint = "http://localhost:12346"; // todo configurable

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
        .UseApplicationInsights()
        .Build();

      host.RunAsService();
#else
      var host = new WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        .UseStartup<Startup>() 
        .UseUrls(Endpoint)
        .Build()
        ;

      host.Run();
#endif
    }
    //private class Application
    //{
    //  private IDisposable _host;

    //  public void Start()
    //  {
    //    _host = WebApp.Start<Startup>(Endpoint);

    //    Console.WriteLine();
    //    Console.WriteLine("Hangfire Server started.");
    //    Console.WriteLine("Dashboard is available at {0}/hangfire", Endpoint);
    //    Console.WriteLine();
    //  }

    //  public void Stop()
    //  {
    //    _host.Dispose();
    //  }
    //}

    //.Start<Startup>(Endpoint))
    //ApplicationBuilderExtensions => ApplicationBuilderExtensions.Start<Startup>(Endpoint)

    //// run hangfire in background 
    //HostFactory.Run(x =>
    //{
    //  x.Service<Application>(s =>
    //  {
    //    s.ConstructUsing(name => new Application());
    //    s.WhenStarted(tc => tc.Start());
    //    s.WhenStopped(tc => tc.Stop());
    //  });
    //  x.RunAsLocalSystem();

    //  x.SetDescription("Hangfire Windows Service Sample");
    //  x.SetDisplayName("Hangfire Windows Service Sample");
    //  x.SetServiceName("hangfire-sample");
    //});
  }
}