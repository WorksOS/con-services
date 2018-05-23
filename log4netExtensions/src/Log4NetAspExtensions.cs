using System;
using System.Diagnostics;
using System.IO;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace VSS.Log4Net.Extensions
{
  public static class Log4NetAspExtensions
  {
    public static void AddLog4Net(this ILoggerFactory loggerFactory, string repoName)
    {
      loggerFactory.AddProvider(new Log4NetProvider());
    }

    public static void ConfigureLog4Net(this IHostingEnvironment appEnv, string configFileRelativePath, string repoName)
    {
      ConfigureLog4Net(repoName, configFileRelativePath, appEnv.ContentRootPath);
    }
    
    public static void ConfigureLog4Net(string repoName, string configFileRelativePath = "log4net.xml", string currentDir = null)
    {
      currentDir = currentDir ?? GetExecutablePath();

      var fullName = Path.Combine(currentDir, configFileRelativePath);
      var configFile = new FileInfo(fullName);
      ILoggerRepository loggerRepo = null;

      foreach (var repo in LogManager.GetAllRepositories())
      {
        if (repo.Name == repoName)
        {
          loggerRepo = repo;
          break;
        }
      }

      if (loggerRepo == null)
      {
        loggerRepo = LogManager.CreateRepository(repoName);
      }

      GlobalContext.Properties["appRoot"] = currentDir;
      XmlConfigurator.Configure(loggerRepo, configFile);
    }

    /// <summary>
    /// Gets the full path to the log4net configuration file which may vary depending on execution context and/or project configuration.
    /// </summary>
    private static string GetExecutablePath()
    {
      string logPath;

      if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "log4net.xml")))
      {
        logPath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Setting GetCurrentDirectory path for the config file {logPath}");
      }
      else if (File.Exists(Path.Combine(AppContext.BaseDirectory, "log4net.xml")))
      {
        logPath = Path.Combine(AppContext.BaseDirectory);
        Console.WriteLine($"Setting BaseDirectory path for the config file {logPath}");
      }
      else
      {
        var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
        logPath = Path.GetDirectoryName(pathToExe);
        Console.WriteLine($"Setting alternative path for the config file {logPath}");
      }

      Console.WriteLine("Log path:" + logPath);

      return logPath;
    }
  }
}
