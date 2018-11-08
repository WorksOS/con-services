using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;

namespace VSS.TRex.Webtools
{
  public class Program
    {
    public static void Main(string[] args)
    {
      var webHost = BuildWebHost(args);

      webHost.Run();
    }

    public static IWebHost BuildWebHost(string[] args)
    {

      return WebHost.CreateDefaultBuilder(args)
        .ConfigureLogging(builder =>
        {
          Log4NetProvider.RepoName = Startup.LoggerRepoName;
          builder.Services.AddSingleton<ILoggerProvider, Log4NetProvider>();
          builder.SetMinimumLevel(LogLevel.Trace);
        })
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
          var env = hostingContext.HostingEnvironment;
          env.ConfigureLog4Net(repoName: Startup.LoggerRepoName, configFileRelativePath: "log4net.xml");

        })
        .UseStartup<Startup>()
        .Build();
    }
  }
}
