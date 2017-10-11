using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

#if NET_4_7
using Topshelf;
using System.Diagnostics;
#endif

namespace VSS.MasterData.Project.WebAPI
{
  /// <summary>
  /// 
  /// </summary>
  public class Program
  {
    /// <summary>
    /// Mains the specified arguments.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public static void Main(string[] args)
    {

      var kestrelConfig = new ConfigurationBuilder()
        .AddJsonFile("kestrelsettings.json", optional: true, reloadOnChange: false)
        .Build();

#if NET_4_7
      HostFactory.Run(x =>
      {
        x.Service<ProjectContainer>(s =>
        {
          s.ConstructUsing(name => new ProjectContainer());
          s.WhenStarted(tc => tc.Start(kestrelConfig));
          s.WhenStopped(tc => tc.Stop());
        });
        x.RunAsLocalSystem();

        x.SetDescription("Project WebAPI, containing various controllers. NET 4.7 port.");
        x.SetDisplayName("ProjectWebAPINet47");
        x.SetServiceName("ProjectWebAPINet47");
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
  internal class ProjectContainer
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

