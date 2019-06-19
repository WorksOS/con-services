using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;

namespace VSS.Hydrology.WebApi
{
  /// <summary>
  /// 
  /// </summary>
  public class Program
  {
    private const string LIBUV_THREAD_COUNT = "LIBUV_THREAD_COUNT";
    private const string MAX_WORKER_THREADS = "MAX_WORKER_THREADS";
    private const string MAX_IO_THREADS = "MAX_IO_THREADS";
    private const string MIN_WORKER_THREADS = "MAX_WORKER_THREADS";
    private const string MIN_IO_THREADS = "MIN_IO_THREADS";
    private const string DEFAULT_CONNECTION_LIMIT = "DEFAULT_CONNECTION_LIMIT";

    /// <summary>
    /// Default program entry point.
    /// </summary>
    public static void Main(string[] args)
    {
      var isService = args.Contains("--service");

      var config = new ConfigurationBuilder()
        .AddCommandLine(args)
        .AddJsonFile("kestrelsettings.json", optional: true, reloadOnChange: false)
        .AddJsonFile("serilog.json", optional: true, reloadOnChange: true)
        .Build();

      var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
      var pathToContentRoot = Path.GetDirectoryName(pathToExe);
      var libuvConfigured = int.TryParse(Environment.GetEnvironmentVariable(LIBUV_THREAD_COUNT), out var libuvThreads);

      var host = new WebHostBuilder().BuildHostWithReflectionException(builder =>
      {
        return builder.UseConfiguration(config)
        .UseKestrel(opts =>
        {
          opts.Limits.MaxResponseBufferSize = 131072; //128K for large exports (default is 64K)
        })
        //.UseUrls("http://127.0.0.1:5002") //DO NOT REMOVE (used for local debugging of long running veta exports)        
        .UseLibuv(opts =>
        {
          if (libuvConfigured)
          {
            opts.ThreadCount = libuvThreads;
          }
        })
        .UseContentRoot(pathToContentRoot)
        .UseStartup<Startup>()
        .ConfigureLogging((hostContext, loggingBuilder) =>
        {
          loggingBuilder.AddProvider(p =>
            new SerilogProvider(
              SerilogExtensions.Configure(config, "VSS.Productivity3D.WebAPI.log"), p.GetService<IHttpContextAccessor>()));
        })
        .Build();
      });

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

      if (!isService)
        host.Run();
      else
        host.RunAsService();
    }
  }
}
