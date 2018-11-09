using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;

namespace RepositoryLandfillTests
{
  public class TestControllerBase
  {
    protected IServiceProvider ServiceProvider;
    

    public void SetupLogging()
    {
      const string loggerRepoName = "UnitTestLogTest";
      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName, "log4nettest.xml");

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      ServiceProvider = new ServiceCollection()
        .AddSingleton<ILoggerProvider, Log4NetProvider>()
        .AddSingleton(loggerFactory)
        .AddLogging()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .BuildServiceProvider();

      Assert.IsNotNull(ServiceProvider.GetService<ILoggerFactory>());
    }
  }
}
