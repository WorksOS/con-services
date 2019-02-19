using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;

namespace VSS.ProfileX.Client.UnitTests
{
  [TestClass]
  public abstract class BaseTestClass
  {
    private IServiceCollection serviceCollection;

    protected IServiceProvider ServiceProvider { get; private set; }

    protected ILogger Log { get; set; }

    [TestInitialize]
    public async Task SetupServices()
    {
      string loggerRepoName = $"Tests::{GetType().Name}";
      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName, "log4nettest.xml");

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory
        .AddDebug()
        .AddConsole(LogLevel.Warning);
      loggerFactory.AddLog4Net(loggerRepoName);

      Log = loggerFactory.CreateLogger(GetType().Name);

      serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();

      var config = serviceCollection.BuildServiceProvider().GetService<IConfigurationStore>();

      SetupTestServices(serviceCollection, config);

      ServiceProvider = serviceCollection.BuildServiceProvider();

      var pretest = await PretestChecks();
      if (!pretest)
      {
        Assert.Inconclusive("Pretest checks for test failed");
      }
    }

    protected abstract IServiceCollection SetupTestServices(IServiceCollection services, IConfigurationStore configuration);

    protected virtual Task<bool> PretestChecks() => Task.FromResult(true);
  }
}