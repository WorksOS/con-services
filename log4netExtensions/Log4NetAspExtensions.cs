using System.IO;
using Microsoft.AspNetCore.Hosting;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Logging;
using log4net.Repository;
using System;

namespace log4netExtensions
{
  public static class Log4NetAspExtensions
  {
    public static void ConfigureLog4Net(this IHostingEnvironment appEnv, string configFileRelativePath, string repoName)
    {
      ConfigureLog4Net(appEnv.ContentRootPath, configFileRelativePath, repoName);
    }

    public static void ConfigureLog4Net(string currentDir, string configFileRelativePath, string repoName)
    {
      var fullName = Path.Combine(currentDir, configFileRelativePath);
      //Console.WriteLine("ConfigureLog4Net: currentDir={0}, configFileRelativePath={1}, fullName={2}", currentDir, configFileRelativePath, fullName);
      FileInfo fi = new FileInfo(fullName);
      ILoggerRepository loggerRepo = null;
      foreach (ILoggerRepository repo in LogManager.GetAllRepositories())
      {
        if (repo.Name == repoName)
        {
          loggerRepo = repo;
        }
      }
      if (loggerRepo == null)
      {
        loggerRepo = LogManager.CreateRepository(repoName);
      }
      GlobalContext.Properties["appRoot"] = currentDir;
      XmlConfigurator.Configure(loggerRepo, fi);
    }

    public static void AddLog4Net(this ILoggerFactory loggerFactory, string repoName)
    {
      loggerFactory.AddProvider(new Log4NetProvider(repoName));
    }
  }
}