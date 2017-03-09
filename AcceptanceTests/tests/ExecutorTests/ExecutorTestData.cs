using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using log4netExtensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using Repositories;
using System.Linq;

namespace RepositoryTests
{
  public class ExecutorTestData
  {
    protected IServiceProvider serviceProvider = null;
    protected AssetRepository assetContext = null;
    protected CustomerRepository customerContext = null;
    protected DeviceRepository deviceContext = null;
    protected ProjectRepository projectContext = null;
    protected SubscriptionRepository subscriptionContext = null;
    protected IRepositoryFactory factory = null;
    protected ILogger logger = null;

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
      logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<AssetIdExecutorTests>();

      assetContext = factory.GetRepository<IAssetEvent>() as AssetRepository;
      customerContext = factory.GetRepository<ICustomerEvent>() as CustomerRepository;
      deviceContext = factory.GetRepository<IDeviceEvent>() as DeviceRepository;
      projectContext = factory.GetRepository<IProjectEvent>() as ProjectRepository;
      subscriptionContext = factory.GetRepository<ISubscriptionEvent>() as SubscriptionRepository;
    }


    protected bool CreateAssetDeviceAssociation(Guid assetUID, long legacyAssetId, Guid? owningCustomerUID, Guid deviceUID, string deviceSerialNumber, string deviceType)
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createAssetEvent = new CreateAssetEvent()
      {
        AssetUID = assetUID,
        AssetName = "The asset Name",
        AssetType = "unknown",
        SerialNumber = "3453gg",
        LegacyAssetId = legacyAssetId,
        OwningCustomerUID = owningCustomerUID,
        ActionUTC = actionUTC
      };

      var createDeviceEvent = new CreateDeviceEvent()
      {
        DeviceUID = deviceUID,
        DeviceSerialNumber = deviceSerialNumber,
        DeviceType = deviceType,
        DeviceState = "active",
        ActionUTC = actionUTC
      };

      var associateDeviceAssetEvent = new AssociateDeviceAssetEvent()
      {
        AssetUID = createAssetEvent.AssetUID,
        DeviceUID = createDeviceEvent.DeviceUID,
        ActionUTC = actionUTC
      };

      assetContext.StoreEvent(createAssetEvent).Wait();
      deviceContext.StoreEvent(createDeviceEvent).Wait();
      deviceContext.StoreEvent(associateDeviceAssetEvent).Wait();
      var g = deviceContext.GetAssociatedAsset(createDeviceEvent.DeviceSerialNumber, createDeviceEvent.DeviceType); g.Wait();
      return (g.Result != null ? true : false);
    }

    protected bool CreateProject(Guid projectUID, int legacyProjectId, Guid customerUID, ProjectType projectType = ProjectType.LandFill)
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var createCustomerEvent = new CreateCustomerEvent()
      { CustomerUID = customerUID, CustomerName = "The Customer Name", CustomerType = CustomerType.Customer.ToString(), ActionUTC = actionUtc };

      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = projectUID,
        ProjectID = legacyProjectId,
        ProjectName = "The Project Name",
        ProjectType = projectType,
        ProjectTimezone = projectTimeZone,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2100, 02, 01),
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))",
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      { CustomerUID = createCustomerEvent.CustomerUID, ProjectUID = createProjectEvent.ProjectUID, LegacyCustomerID = 1234, RelationType = RelationType.Customer, ActionUTC = actionUtc };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      var g = projectContext.GetProject(legacyProjectId); g.Wait();
      return (g.Result != null ? true : false);
    }

    protected bool CreateProjectSub(Guid projectUID, Guid customerUID, string subToInsert)
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = subToInsert,
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUtc
      };

      var associateProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent()
      {
        SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID,
        ProjectUID = projectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUtc
      };

      subscriptionContext.StoreEvent(createProjectSubscriptionEvent).Wait();
      var s = subscriptionContext.StoreEvent(associateProjectSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "associateProjectSubscription event not written");
      var g = projectContext.GetProjectBySubcription(createProjectSubscriptionEvent.SubscriptionUID.ToString()); g.Wait();
      return (g.Result != null ? true : false);
    }

    protected bool CreateAssetSub(Guid assetUID, Guid customerUID, string subToInsert)
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createAssetSubscriptionEvent = new CreateAssetSubscriptionEvent()
      {
        AssetUID = assetUID,
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        DeviceUID = null,
        SubscriptionType = subToInsert,
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUtc
      };

      var s = subscriptionContext.StoreEvent(createAssetSubscriptionEvent);      
      s.Wait();
      Assert.AreEqual(1, s.Result, "createAssetSubscriptionEvent event not written");
      var g = subscriptionContext.GetSubscriptionsByAsset(createAssetSubscriptionEvent.AssetUID.ToString(), DateTime.UtcNow.Date); g.Wait();
      return (g.Result != null ? true : false);
    }
    
    protected bool CreateCustomer(Guid customerUID, string TccOrgId)
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      bool areWrittenOk = false;

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = customerUID,
        CustomerName = "the name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var s = customerContext.StoreEvent(createCustomerEvent); s.Wait();
      Assert.AreEqual(1, s.Result, "createCustomerEvent event not written");
      var g = customerContext.GetCustomerWithTccOrg(createCustomerEvent.CustomerUID); g.Wait();
      areWrittenOk = (g.Result != null ? true : false);

      if (areWrittenOk && !string.IsNullOrEmpty(TccOrgId))
      {
        var createCustomerTccOrgEvent = new CreateCustomerTccOrgEvent()
        {
          CustomerUID = customerUID,
          TCCOrgID = TccOrgId,
          ActionUTC = actionUtc
        };

        s = customerContext.StoreEvent(createCustomerTccOrgEvent); s.Wait();
        Assert.AreEqual(1, s.Result, "createCustomerTccOrgEvent event not written");
        g = customerContext.GetCustomerWithTccOrg(createCustomerEvent.CustomerUID); g.Wait();
        areWrittenOk = (g.Result != null ? true : false);
      }
      return areWrittenOk;
    }

    protected bool CreateCustomerSub(Guid customerUID, string subToInsert)
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerSubscriptionEvent = new CreateCustomerSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = subToInsert.ToString(),
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUtc
      };

      var s = subscriptionContext.StoreEvent(createCustomerSubscriptionEvent); s.Wait();
      Assert.AreEqual(1, s.Result, "associateCustomerSubscription event not written");
      var g = subscriptionContext.GetSubscription(createCustomerSubscriptionEvent.SubscriptionUID.ToString()); g.Wait();
      return (g.Result != null ? true : false);
    }
  }

}

