using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.WebApi.Common;

namespace MockProjectWebApi
{
  public class Program
  {
    private const string LIBUV_THREAD_COUNT = "LIBUV_THREAD_COUNT";
    private const string MAX_WORKER_THREADS = "MAX_WORKER_THREADS";
    private const string MAX_IO_THREADS = "MAX_IO_THREADS";
    private const string MIN_WORKER_THREADS = "MAX_WORKER_THREADS";
    private const string MIN_IO_THREADS = "MIN_IO_THREADS";
    private const string DEFAULT_CONNECTION_LIMIT = "DEFAULT_CONNECTION_LIMIT";

    public static void Main()
    {
      var host = CreateHostBuilder(null).Build();

      var configuration = host.Services.GetRequiredService<IConfigurationStore>();

      var log = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
      log.LogInformation("Productivity3D service starting");

      if (int.TryParse(configuration.GetValueString(MAX_WORKER_THREADS), out var maxWorkers) &&
          int.TryParse(configuration.GetValueString(MAX_IO_THREADS), out var maxIo))
      {
        ThreadPool.SetMaxThreads(maxWorkers, maxIo);
        log.LogInformation($"Max Worker Threads = {maxWorkers}");
        log.LogInformation($"Max IO Threads = {maxIo}");
      }
      else
      {
        log.LogInformation("Max Worker Threads = Default");
        log.LogInformation("Max IO Threads = Default");
      }

      if (int.TryParse(configuration.GetValueString(MIN_WORKER_THREADS), out var minWorkers) &&
          int.TryParse(configuration.GetValueString(MIN_IO_THREADS), out var minIo))
      {
        ThreadPool.SetMinThreads(minWorkers, minIo);
        log.LogInformation($"Min Worker Threads = {minWorkers}");
        log.LogInformation($"Min IO Threads = {minIo}");
      }
      else
      {
        log.LogInformation("Min Worker Threads = Default");
        log.LogInformation("Min IO Threads = Default");
      }

      if (int.TryParse(configuration.GetValueString(DEFAULT_CONNECTION_LIMIT), out var connectionLimit))
      {
        //Check how many requests we can execute
        ServicePointManager.DefaultConnectionLimit = connectionLimit;
        log.LogInformation($"Default connection limit = {connectionLimit}");
      }
      else
      {
        log.LogInformation("Default connection limit = Default");
      }

      host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
          .ConfigureWebHostDefaults(webBuilder =>
          {
            webBuilder.UseLibuv(opts => opts.ThreadCount = 32)
                      .BuildKestrelWebHost()
                      .UseStartup<Startup>()
                      .UseUrls("http://0.0.0.0:5001");
          })
    ;
  }
}
