using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ExecutorBaseTests
  {
    public IServiceProvider serviceProvider = null;
    protected AssetRepository assetRepository;
    protected DeviceRepository deviceRepository;
    protected CustomerRepository customerRepository;
    protected ProjectRepository projectRepository;
    protected SubscriptionRepository subscriptionsRepository;

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
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.AddTransient<IRepository<IAssetEvent>, AssetRepository>()
        .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
        .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
        .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>();
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
      serviceProvider = serviceCollection.BuildServiceProvider();


      assetRepository = serviceProvider.GetRequiredService<IRepository<IAssetEvent>>() as AssetRepository;
      deviceRepository = serviceProvider.GetRequiredService<IRepository<IDeviceEvent>>() as DeviceRepository;
      customerRepository = serviceProvider.GetRequiredService<IRepository<ICustomerEvent>>() as CustomerRepository;
      projectRepository = serviceProvider.GetRequiredService<IRepository<IProjectEvent>>() as ProjectRepository;
      subscriptionsRepository = serviceProvider.GetRequiredService<IRepository<ISubscriptionEvent>>() as SubscriptionRepository;
    }
  
  }
}
