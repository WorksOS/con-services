using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Repositories;
using VSS.GenericConfiguration;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.Executors;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using log4netExtensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace ExecutorTests
{
  [TestClass]
  public class TagFileProcessingErrorExecutorTests
  {
    IServiceProvider serviceProvider = null;
    AssetRepository assetContext = null;
    CustomerRepository customerContext = null;
    DeviceRepository deviceContext = null;
    ProjectRepository projectContext = null;
    SubscriptionRepository subscriptionContext = null;
    IRepositoryFactory factory = null;
    ILogger logger = null;

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
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory)
          .AddSingleton<IConfigurationStore, GenericConfiguration>()
          .AddSingleton<IRepositoryFactory, RepositoryFactory>()
          .AddTransient<IRepository<IAssetEvent>, AssetRepository>()
          .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
          .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
          .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
          .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
          .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>();

      serviceProvider = serviceCollection.BuildServiceProvider();
      factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<TagFileProcessingErrorExecutorTests>();
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutor()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(1, "Data from my dozer", TagFileErrorsEnum.ProjectID_NoMatchingArea);
      request.Validate();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory, logger).Process(request) as TagFileProcessingErrorResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.result, "unsuccessful");
    }
  }
}
