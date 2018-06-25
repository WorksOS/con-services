using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
{
  public class ExecutorTestData
  {
    protected IServiceProvider serviceProvider;
    protected ILoggerFactory LoggerFactory;
    protected ILogger logger;
    protected IConfigurationStore configStore;

    protected AssetRepository assetRepo;
    protected DeviceRepository deviceRepo;
    protected CustomerRepository customerRepo;
    protected ProjectRepository projectRepo;
    protected SubscriptionRepository subscriptionRepo;

    protected IKafka producer;
    protected string kafkaTopicName;
    private readonly string loggerRepoName = "UnitTestLogTest";

    [TestInitialize]
    public virtual void InitTest()
    {
      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName, "log4nettest.xml");
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging();
      serviceCollection
        .AddSingleton(loggerFactory)
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IRepository<IAssetEvent>, AssetRepository>()
        .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
        .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
        .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>()
        .AddSingleton<IKafka, RdKafkaDriver>(); 

      serviceProvider = serviceCollection.BuildServiceProvider();

      this.LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
      logger = loggerFactory.CreateLogger<AssetIdExecutorTests>();

      //logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<AssetIdExecutorTests>();
      assetRepo = serviceProvider.GetRequiredService<IRepository<IAssetEvent>>() as AssetRepository;
      configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      deviceRepo = serviceProvider.GetRequiredService<IRepository<IDeviceEvent>>() as DeviceRepository;
      customerRepo = serviceProvider.GetRequiredService<IRepository<ICustomerEvent>>() as CustomerRepository;
      projectRepo = serviceProvider.GetRequiredService<IRepository<IProjectEvent>>() as ProjectRepository;
      subscriptionRepo = serviceProvider.GetRequiredService<IRepository<ISubscriptionEvent>>() as SubscriptionRepository;

      kafkaTopicName = "AcceptanceTestKafkaName";
      producer = new RdKafkaDriver();
      if (!producer.IsInitializedProducer)
        producer.InitProducer(configStore);
    }

    protected bool CreateAssetDeviceAssociation(Guid assetUid, long legacyAssetId, Guid? owningCustomerUid, Guid deviceUid, string deviceSerialNumber, string deviceType)
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createAssetEvent = new CreateAssetEvent
      {
        AssetUID = assetUid,
        AssetName = "The asset Name",
        AssetType = "unknown",
        SerialNumber = "3453gg",
        LegacyAssetId = legacyAssetId,
        OwningCustomerUID = owningCustomerUid,
        ActionUTC = actionUtc
      };

      var createDeviceEvent = new CreateDeviceEvent
      {
        DeviceUID = deviceUid,
        DeviceSerialNumber = deviceSerialNumber,
        DeviceType = deviceType,
        DeviceState = "active",
        ActionUTC = actionUtc
      };

      var associateDeviceAssetEvent = new AssociateDeviceAssetEvent
      {
        AssetUID = createAssetEvent.AssetUID,
        DeviceUID = createDeviceEvent.DeviceUID,
        ActionUTC = actionUtc
      };

      assetRepo.StoreEvent(createAssetEvent).Wait();
      deviceRepo.StoreEvent(createDeviceEvent).Wait();
      deviceRepo.StoreEvent(associateDeviceAssetEvent).Wait();
      var g = deviceRepo.GetAssociatedAsset(createDeviceEvent.DeviceSerialNumber, createDeviceEvent.DeviceType); g.Wait();
      return g.Result != null;
    }

    protected bool CreateProject(Guid projectUid, int legacyProjectId, Guid customerUid, ProjectType projectType = ProjectType.LandFill, string projectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))")
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      //var createCustomerEvent = new CreateCustomerEvent()
      //{ CustomerUID = customerUID, CustomerName = "The Customer Name", CustomerType = CustomerType.Customer.ToString(), ActionUTC = actionUtc };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = projectUid,
        ProjectID = legacyProjectId,
        ProjectName = "The Project Name",
        ProjectType = projectType,
        ProjectTimezone = projectTimeZone,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2100, 02, 01),
        ProjectBoundary = projectBoundary,
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer { CustomerUID = customerUid, ProjectUID = createProjectEvent.ProjectUID, LegacyCustomerID = 1234, RelationType = RelationType.Customer, ActionUTC = actionUtc };

      projectRepo.StoreEvent(createProjectEvent).Wait();
      projectRepo.StoreEvent(associateCustomerProjectEvent).Wait();

      var g = projectRepo.GetProject(legacyProjectId); g.Wait();
      return g.Result != null;
    }

    protected bool DeleteProject(Guid projectUid)
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var deleteProjectEvent = new DeleteProjectEvent()
      {
        ProjectUID = projectUid,
        ActionUTC = actionUtc
      };

      projectRepo.StoreEvent(deleteProjectEvent).Wait();

      var g = projectRepo.GetProject(projectUid.ToString()); g.Wait();
      return g.Result == null;
    }

    protected bool CreateProjectSub(Guid projectUid, Guid customerUid, string subToInsert)
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent
      {
        CustomerUID = customerUid,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = subToInsert,
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUtc
      };

      var associateProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent
      {
        SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID,
        ProjectUID = projectUid,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUtc
      };

      subscriptionRepo.StoreEvent(createProjectSubscriptionEvent).Wait();
      var s = subscriptionRepo.StoreEvent(associateProjectSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "associateProjectSubscription event not written");
      var g = projectRepo.GetProjectBySubcription(createProjectSubscriptionEvent.SubscriptionUID.ToString()); g.Wait();
      return g.Result != null;
    }

    protected bool CreateAssetSub(Guid assetUid, Guid customerUid, string subToInsert)
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createAssetSubscriptionEvent = new CreateAssetSubscriptionEvent
      {
        AssetUID = assetUid,
        CustomerUID = customerUid,
        SubscriptionUID = Guid.NewGuid(),
        DeviceUID = null,
        SubscriptionType = subToInsert,
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUtc
      };

      var s = subscriptionRepo.StoreEvent(createAssetSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "createAssetSubscriptionEvent event not written");
      var g = subscriptionRepo.GetSubscriptionsByAsset(createAssetSubscriptionEvent.AssetUID.ToString(), DateTime.UtcNow.Date); g.Wait();
      return g.Result != null;
    }

    protected bool CreateCustomer(Guid customerUid, string tccOrgId, CustomerType customerType = CustomerType.Customer)
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      bool areWrittenOk;

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = customerUid,
        CustomerName = "the name",
        CustomerType = customerType.ToString(),
        ActionUTC = actionUtc
      };

      var s = customerRepo.StoreEvent(createCustomerEvent); s.Wait();
      Assert.AreEqual(1, s.Result, "createCustomerEvent event not written");
      var g = customerRepo.GetCustomerWithTccOrg(createCustomerEvent.CustomerUID); g.Wait();
      areWrittenOk = g.Result != null;

      if (areWrittenOk && !string.IsNullOrEmpty(tccOrgId))
      {
        var createCustomerTccOrgEvent = new CreateCustomerTccOrgEvent
        {
          CustomerUID = customerUid,
          TCCOrgID = tccOrgId,
          ActionUTC = actionUtc
        };

        s = customerRepo.StoreEvent(createCustomerTccOrgEvent); s.Wait();
        Assert.AreEqual(1, s.Result, "createCustomerTccOrgEvent event not written");
        g = customerRepo.GetCustomerWithTccOrg(createCustomerEvent.CustomerUID); g.Wait();
        areWrittenOk = g.Result != null;
      }
      return areWrittenOk;
    }

    protected bool CreateCustomerSub(Guid customerUid, string subToInsert)
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerSubscriptionEvent = new CreateCustomerSubscriptionEvent
      {
        CustomerUID = customerUid,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = subToInsert,
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUtc
      };

      var s = subscriptionRepo.StoreEvent(createCustomerSubscriptionEvent); s.Wait();
      Assert.AreEqual(1, s.Result, "associateCustomerSubscription event not written");
      var g = subscriptionRepo.GetSubscription(createCustomerSubscriptionEvent.SubscriptionUID.ToString()); g.Wait();
      return g.Result != null;
    }
  }
}
