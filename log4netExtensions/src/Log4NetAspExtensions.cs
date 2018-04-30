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
    public static void ConfigureLog4Net(this IHostingEnvironment appEnv, string configFileRelativePath, string repoName)
    {
      ConfigureLog4Net(appEnv.ContentRootPath, configFileRelativePath, repoName);
    }

    public static void ConfigureLog4Net(string currentDir, string configFileRelativePath, string repoName)
    {
      var fullName = Path.Combine(currentDir, configFileRelativePath);
      var configFile = new FileInfo(fullName);
      ILoggerRepository loggerRepo = null;

      foreach (var repo in LogManager.GetAllRepositories())
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
      XmlConfigurator.Configure(loggerRepo, configFile);
    }

    public static void AddLog4Net(this ILoggerFactory loggerFactory, string repoName)
    {
      loggerFactory.AddProvider(new Log4NetProvider(null));
    }

  }
}