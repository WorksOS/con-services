using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockClasses;
using System;
using log4netExtensions;
using VSS.Masterdata;
using VSS.GenericConfiguration;

namespace VSS.TagFileAuth.Service.WebApiTests.Executors
{
  [TestClass]
  public class ExecutorBaseTests
  {
    public IServiceProvider serviceProvider = null;

    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();

      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceCollection.AddSingleton<IRepositoryFactory, MockFactory>();
      serviceCollection.AddSingleton<IConfigurationStore, VSS.GenericConfiguration.GenericConfiguration>();
      serviceProvider = serviceCollection.BuildServiceProvider();

     // var f = serviceProvider.GetRequiredService<IRepositoryFactory>();
    }

    [TestMethod]
    public void GetFactory()
    {
      IRepositoryFactory factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      Assert.IsNotNull(factory, "Unable to retrieve factory from DI");
    }

    [TestMethod]
    public void GetLogger()
    {
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
      Assert.IsNotNull(loggerFactory, "Unable to retrieve loggerFactory from DI");
    }
  }
}
