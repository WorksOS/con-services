using System;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;

namespace MockProjectWebApi
{
  public class Program
  {
    const string LIBUV_THREAD_COUNT = "LIBUV_THREAD_COUNT";
    const string MAX_WORKER_THREADS = "MAX_WORKER_THREADS";
    const string MAX_IO_THREADS = "MAX_IO_THREADS";
    const string MIN_WORKER_THREADS = "MAX_WORKER_THREADS";
    const string MIN_IO_THREADS = "MIN_IO_THREADS";
    const string DEFAULT_CONNECTION_LIMIT = "DEFAULT_CONNECTION_LIMIT";

    public static void Main(string[] args)
    {
      //var host = new WebHostBuilder()
      //    .UseKestrel()
      //    .UseContentRoot(Directory.GetCurrentDirectory())
      //    .UseIISIntegration()
      //    .UseStartup<Startup>()
      //    .UseUrls("http://0.0.0.0:5001")
      //    .Build();

      //host.Run();

      var config = new ConfigurationBuilder()
        .AddJsonFile("kestrelsettings.json", optional: true, reloadOnChange: false)
        .Build();

      var libuvConfigured = int.TryParse(Environment.GetEnvironmentVariable(LIBUV_THREAD_COUNT), out var libuvThreads);
      var host = new WebHostBuilder()
        .UseConfiguration(config)
        .UseKestrel()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .ConfigureLogging(builder =>
        {
          Log4NetProvider.RepoName = Startup.LOGGER_REPO_NAME;
          builder.Services.AddSingleton<ILoggerProvider, Log4NetProvider>();
          builder.SetMinimumLevel(LogLevel.Debug);
          builder.AddConfiguration(config);
        })
       // .UseIISIntegration()
        .UseLibuv(opts =>
        {
          if (libuvConfigured)
          {
            opts.ThreadCount = libuvThreads;
          }
        })
        .UseStartup<Startup>()
        .UseUrls("http://0.0.0.0:5001")
        .Build();

      var log = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
      log.LogInformation("Productivity3D service starting");
      log.LogInformation($"Num Libuv Threads = {(libuvConfigured ? libuvThreads.ToString() : "Default")}");

      if (int.TryParse(Environment.GetEnvironmentVariable(MAX_WORKER_THREADS), out var maxWorkers) &&
          int.TryParse(Environment.GetEnvironmentVariable(MAX_IO_THREADS), out var maxIo))
      {
        ThreadPool.SetMaxThreads(maxWorkers, maxIo);
        log.LogInformation($"Max Worker Threads = {maxWorkers}");
        log.LogInformation($"Max IO Threads = {maxIo}");
      }
      else
      {
        log.LogInformation($"Max Worker Threads = Default");
        log.LogInformation($"Max IO Threads = Default");
      }

      if (int.TryParse(Environment.GetEnvironmentVariable(MIN_WORKER_THREADS), out var minWorkers) &&
          int.TryParse(Environment.GetEnvironmentVariable(MIN_IO_THREADS), out var minIo))
      {
        ThreadPool.SetMinThreads(minWorkers, minIo);
        log.LogInformation($"Min Worker Threads = {minWorkers}");
        log.LogInformation($"Min IO Threads = {minIo}");
      }
      else
      {
        log.LogInformation($"Min Worker Threads = Default");
        log.LogInformation($"Min IO Threads = Default");
      }

      if (int.TryParse(Environment.GetEnvironmentVariable(DEFAULT_CONNECTION_LIMIT), out var connectionLimit))
      {
        //Check how many requests we can execute
        ServicePointManager.DefaultConnectionLimit = connectionLimit;
        log.LogInformation($"Default connection limit = {connectionLimit}");
      }
      else
      {
        log.LogInformation($"Default connection limit = Default");
      }

      host.Run();
    }
  }
}
