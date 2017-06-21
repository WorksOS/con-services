using log4netExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.GenericConfiguration;

namespace RepositoryTests.Internal
{
  public class TestControllerBase
  {
    protected IServiceProvider ServiceProvider;

    public void SetupLogging()
    {
      const string loggerRepoName = "UnitTestLogTest";
      var logPath = Directory.GetCurrentDirectory();

      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      ServiceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(loggerFactory)
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .BuildServiceProvider();

      Assert.IsNotNull(ServiceProvider.GetService<ILoggerFactory>());
    }
  }
}