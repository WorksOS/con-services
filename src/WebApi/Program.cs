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


      var host = new WebHostBuilder()
        .UseConfiguration(config)
        .UseKestrel()
        .UseLibuv(opts =>
        {
          if (Int32.TryParse(Environment.GetEnvironmentVariable(MAX_WORKER_THREADS), out int libuvThreads))
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

      if (Int32.TryParse(Environment.GetEnvironmentVariable(MAX_WORKER_THREADS), out int maxWorkers) &&
          Int32.TryParse(Environment.GetEnvironmentVariable(MAX_WORKER_THREADS), out int maxIo))
      {
        ThreadPool.SetMaxThreads(maxWorkers, maxIo);
      }

      if (Int32.TryParse(Environment.GetEnvironmentVariable(MIN_WORKER_THREADS), out int minWorkers) &&
          Int32.TryParse(Environment.GetEnvironmentVariable(MIN_WORKER_THREADS), out int minIo))
      {
        ThreadPool.SetMinThreads(minWorkers, minIo);
      }
             

      if (Int32.TryParse(Environment.GetEnvironmentVariable(MIN_WORKER_THREADS), out int connectionLimit))
      {
        //Check how many requests we can execute
        ServicePointManager.DefaultConnectionLimit = connectionLimit;
      }

      if (!isService)
        host.Run();
      else
        host.RunAsService();
    }
  }
}