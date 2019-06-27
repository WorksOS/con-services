using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VSS.Hydrology.Tests.Ponding.Executors
{
  [TestClass]
  public class PondingExecutorTests 
  {
    //private static IServiceProvider serviceProvider;
    //private static ILoggerFactory logger;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      //ILoggerFactory loggerFactory = new LoggerFactory();
      //loggerFactory.AddDebug();

      //var serviceCollection = new ServiceCollection();
      //serviceCollection.AddLogging();
      //serviceCollection.AddSingleton(loggerFactory);
      //serviceCollection
      //  .AddSingleton<IConfigurationStore, GenericConfiguration>()
      //  .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
      //  .AddTransient<IErrorCodesProvider, RaptorResult>();

      //serviceProvider = serviceCollection.BuildServiceProvider();

      //logger = serviceProvider.GetRequiredService<ILoggerFactory>();
    }

    [TestMethod]
    public void InvalidRequest_fail()
    {
      Assert.AreEqual(1,2);
    }

    
  }
}
