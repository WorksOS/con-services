using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;


#if NET_4_7
using Topshelf;
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

      var kestrelConfig = new ConfigurationBuilder()
        .AddJsonFile("kestrelsettings.json", optional: true, reloadOnChange: false)
        .Build();

#if NET_4_7 //To run the service use https://docs.microsoft.com/en-us/aspnet/core/hosting/windows-service
      HostFactory.Run(x =>
      {
        x.Service<FilterContainer>(s =>
        {
          s.ConstructUsing(name => new FilterContainer());
          s.WhenStarted(tc => tc.Start(kestrelConfig));
          s.WhenStopped(tc => tc.Stop());
        });
        x.RunAsLocalSystem();

        x.SetDescription("Filter WebAPI, containing various controllers. NET 4.7 port.");
        x.SetDisplayName("FilterWebAPINet47");
        x.SetServiceName("FilterWebAPINet47");
        x.EnableServiceRecovery(c =>
        {
          c.RestartService(1);
          c.OnCrashOnly();
        });
      });
#else
      var host = new WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        .UseStartup<Startup>()
        .Build();

      host.Run();
#endif
    }
  }

#if NET_4_7
  internal class FilterContainer
  {
    private IWebHost _webHost;

    /// <summary>
    /// 
    /// </summary>
    public void Start(IConfiguration config)
    {
      var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
      var pathToContentRoot = Path.GetDirectoryName(pathToExe);

      _webHost = new WebHostBuilder()
        .UseKestrel()
        .UseConfiguration(config)
        .UseContentRoot(pathToContentRoot)
        .UseStartup<Startup>()
        .Build();

      _webHost.Start();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Stop()
    {
      _webHost?.Dispose();
    }
  }
#endif

}