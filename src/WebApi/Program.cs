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

namespace VSS.Productivity3D.WebApi
{
  /// <summary>
  /// 
  /// </summary>
  public class Program
  {

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
          opts.ThreadCount = 32;
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

      ThreadPool.SetMaxThreads(1024, 2048);
      ThreadPool.SetMinThreads(1024, 2048);

      //Check how many requests we can execute
      ServicePointManager.DefaultConnectionLimit = 128;

      if (!isService)
        host.Run();
      else
        host.RunAsService();
    }
  }
}