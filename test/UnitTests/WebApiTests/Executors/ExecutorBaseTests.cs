using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using log4netExtensions;
using Repositories;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

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
      serviceCollection.AddSingleton<IRepositoryFactory, RepositoryFactory>()
                        .AddTransient<IRepository<IAssetEvent>, AssetRepository>()
                        .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
                        .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
                        .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
                        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
                        .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>();
      serviceCollection.AddSingleton<IConfigurationStore, VSS.GenericConfiguration.GenericConfiguration>();
      serviceProvider = serviceCollection.BuildServiceProvider();
    }
  
  }
}
