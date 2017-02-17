using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockClasses;
using System;
using log4netExtensions;
using VSS.TagFileAuth.Service.Repositories.Interfaces;

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

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net("UnitTestLogTest");

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceCollection.AddSingleton<IRepositoryFactory, MockFactory>();
      serviceProvider = serviceCollection.BuildServiceProvider();
    }
  }
}
