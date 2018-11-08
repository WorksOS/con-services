using System.Collections;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;
using VSS.WebApi.Common;

namespace VSS.Tile.Service.WebApi
{
  public class Program
  {
    public static void Main(string[] args)
    {
      BuildWebHost(args).Run();
    }

    public static IWebHost BuildWebHost(string[] args)
    {
      var kestrelConfig = new ConfigurationBuilder()
        .AddJsonFile("kestrelsettings.json", true, false).Build();

      Log4NetAspExtensions.ConfigureLog4Net(Startup.LOGGER_REPO_NAME);

      var host = new WebHostBuilder().BuildHostWithReflectionException(hostBuilder =>
      {
        return hostBuilder.UseKestrel()
          .UseLibuv(opts => { opts.ThreadCount = 32; })
          .UseContentRoot(Directory.GetCurrentDirectory())
          .UseConfiguration(kestrelConfig)
          .ConfigureLogging(builder =>
          {
            Log4NetProvider.RepoName = Startup.LOGGER_REPO_NAME;
            builder.Services.AddSingleton<ILoggerProvider, Log4NetProvider>();
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddConfiguration(kestrelConfig);
          })

          .UsePrometheus()
          .UseStartup<Startup>()
          .Build();
      });

      ThreadPool.SetMaxThreads(1024, 2048);
      ThreadPool.SetMinThreads(1024, 2048);

      //Check how many requests we can execute
      ServicePointManager.DefaultConnectionLimit = 128;

      var log = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
      log.LogInformation("Tile service starting");
      log.LogInformation("*************CONFIGURATION DETAILS*******************");
      foreach (DictionaryEntry entry in System.Environment.GetEnvironmentVariables())
      {
        log.LogInformation(entry.Key + ":" + entry.Value);
      }

      return host;
    }
  }
}
