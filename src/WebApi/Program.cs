using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;
using System;

namespace VSS.Productivity3D.WebApi
{
  /// <summary>
  /// 
  /// </summary>
  public class Program
  {
    const string LIBUV_THREAD_COUNT = "LIBUV_THREAD_COUNT";
    const string MAX_WORKER_THREADS = "MAX_WORKER_THREADS";
    const string MAX_IO_THREADS = "MAX_IO_THREADS";
    const string MIN_WORKER_THREADS = "MAX_WORKER_THREADS";
    const string MIN_IO_THREADS = "MIN_IO_THREADS";
    const string DEFAULT_CONNECTION_LIMIT = "DEFAULT_CONNECTION_LIMIT";


    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
      bool isService = args.Contains("--service");

      var config = new ConfigurationBuilder()
        .AddCommandLine(args)
        .Build();

      //To run the service use https://docs.microsoft.com/en-us/aspnet/core/hosting/windows-service
      var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
      var pathToContentRoot = Path.GetDirectoryName(pathToExe);

      bool libuvConfigured = Int32.TryParse(Environment.GetEnvironmentVariable(LIBUV_THREAD_COUNT), out int libuvThreads);
      var host = new WebHostBuilder()
        .UseConfiguration(config)
        .UseKestrel()
        //.UseUrls("http://127.0.0.1:5002") //DO NOT REMOVE (used for local debugging of long running veta exports)
        .UseLibuv(opts =>
        {
          if (libuvConfigured)
          {
            opts.ThreadCount = libuvThreads;
          }
          
        })
        .UseContentRoot(pathToContentRoot)
        .ConfigureLogging(builder =>
        {
          Log4NetProvider.RepoName = Startup.LOGGER_REPO_NAME;
          builder.Services.AddSingleton<ILoggerProvider, Log4NetProvider>();
          builder.SetMinimumLevel(LogLevel.Trace);
        })
        .UseStartup<Startup>()
        .Build();

      var log = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
      log.LogInformation("Productivity3D service starting");
      log.LogInformation($"Num Libuv Threads = {(libuvConfigured ? libuvThreads.ToString() : "Default")}");
      


      if (Int32.TryParse(Environment.GetEnvironmentVariable(MAX_WORKER_THREADS), out int maxWorkers) &&
          Int32.TryParse(Environment.GetEnvironmentVariable(MAX_IO_THREADS), out int maxIo))
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

      if (Int32.TryParse(Environment.GetEnvironmentVariable(MIN_WORKER_THREADS), out int minWorkers) &&
          Int32.TryParse(Environment.GetEnvironmentVariable(MIN_IO_THREADS), out int minIo))
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
             

      if (Int32.TryParse(Environment.GetEnvironmentVariable(DEFAULT_CONNECTION_LIMIT), out int connectionLimit))
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
