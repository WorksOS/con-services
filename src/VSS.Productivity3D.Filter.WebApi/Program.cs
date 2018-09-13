using System;
using System.Collections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using VSS.Log4Net.Extensions;
using VSS.WebApi.Common;


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


      var host = new WebHostBuilder().BuildHostWithReflectionException(hostBuilder =>
      {
        return hostBuilder.UseKestrel()
          .UseLibuv(opts => { opts.ThreadCount = 32; })
          .UseContentRoot(Directory.GetCurrentDirectory())
          .UseConfiguration(kestrelConfig)
          .ConfigureLogging(builder =>
          {
            Log4NetProvider.RepoName = Startup.LoggerRepoName;
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
      log.LogInformation("3D Filter service starting");
      log.LogInformation("*************CONFIGURATION DETAILS*******************");
      foreach (DictionaryEntry entry in System.Environment.GetEnvironmentVariables())
      {
        log.LogInformation(entry.Key + ":" + entry.Value);
      }

      host.Run();
    }
  }
}
