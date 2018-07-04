using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using System.Threading;
using VSS.Log4Net.Extensions;
using VSS.WebApi.Common;

#if NET_4_7
using Microsoft.AspNetCore.Hosting.WindowsServices;
using System.Diagnostics;
#endif


namespace VSS.Productivity3D.TagFileAuth.WebAPI
{
  /// <summary>
  /// Application entry point.
  /// </summary>
  public class Program
  {
    /// <summary>
    /// Default constructory.
    /// </summary>
    public static void Main(string[] args)
    {
      var config = new ConfigurationBuilder()
        .AddCommandLine(args)
        .Build();

#if NET_4_7
      //To run the service use https://docs.microsoft.com/en-us/aspnet/core/hosting/windows-service
      var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
      var pathToContentRoot = Path.GetDirectoryName(pathToExe);


      var host = new WebHostBuilder()
        .UseConfiguration(config)
        .UseKestrel()
        .UseContentRoot(pathToContentRoot)
        .ConfigureLogging(builder =>
        {
          Log4NetProvider.RepoName = Startup.LOGGER_REPO_NAME;
          builder.Services.AddSingleton<ILoggerProvider, Log4NetProvider>();
          builder.SetMinimumLevel(LogLevel.Trace);
        })
        .UseIISIntegration()
        .UseStartup<Startup>();
      host.Build().RunAsService();
#else

      var host = new WebHostBuilder()
        .UseKestrel()
        .UseLibuv(opts =>
        {
          opts.ThreadCount = 32;
        })
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        .ConfigureLogging(builder =>
        {
          Log4NetProvider.RepoName = Startup.LoggerRepoName;
          builder.Services.AddSingleton<ILoggerProvider, Log4NetProvider>();
          builder.SetMinimumLevel(LogLevel.Debug);
        })
        .UsePrometheus()
        .UseStartup<Startup>()
        .Build();

      ThreadPool.SetMaxThreads(1024, 2048);
      ThreadPool.SetMinThreads(1024, 2048);

      //Check how many requests we can execute
      ServicePointManager.DefaultConnectionLimit = 128;
      host.Run();
#endif
    }
  }
}
