using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling;

namespace VSS.MasterData.Models.UnitTests
{
  [TestClass]
  public class BaseTest
  {
    protected IServiceProvider ServiceProvider;

    /// <summary>
    /// Initializes the test.
    /// </summary>
    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();

      string loggerRepoName = "UnitTestLogTest";
      var logPath = Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceCollection
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ErrorCodesProvider>();

      ServiceProvider = serviceCollection.BuildServiceProvider();
    }
  }
}
