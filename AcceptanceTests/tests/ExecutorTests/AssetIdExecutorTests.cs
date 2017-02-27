using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using log4netExtensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.GenericConfiguration;
using VSS.Masterdata;
using MasterDataConsumer;
using VSS.Device.Data;
using VSS.Asset.Data;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.Models;
using VSS.TagFileAuth.Service.WebApiModels.Interfaces;
using VSS.TagFileAuth.Service.WebApiModels.Executors;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.Customer.Data;
using VSS.Geofence.Data;
using VSS.Project.Data;
using VSS.Project.Service.Repositories;

namespace RepositoryTests
{
  [TestClass]
  public class AssetIdExecutorTests
  {
    IServiceProvider serviceProvider = null;
    AssetRepository assetContext = null;
    CustomerRepository customerContext = null;
    DeviceRepository deviceContext = null;
    ProjectRepository projectContext = null;
    SubscriptionRepository subscriptionContext = null;
    IRepositoryFactory factory = null;

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

      assetContext = factory.GetRepository<IAssetEvent>() as AssetRepository;
      customerContext = factory.GetRepository<ICustomerEvent>() as CustomerRepository;
      deviceContext = factory.GetRepository<IDeviceEvent>() as DeviceRepository;      
      projectContext = factory.GetRepository<IProjectEvent>() as ProjectRepository;
      subscriptionContext = factory.GetRepository<ISubscriptionEvent>() as SubscriptionRepository;
    }


    [TestMethod]
    public void ValidateGetAssetIdRequest()
    {
      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "");
      bool isValid = assetIdRequest.Validate();
      Assert.IsFalse(isValid, "must be at least projectID or radioSerial");

      assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "ASerial5");
      isValid = assetIdRequest.Validate();
      Assert.IsFalse(isValid, "must be valid deviceType for radioSerial");

      assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 100, "ASerial5");
      isValid = assetIdRequest.Validate();
      Assert.IsFalse(isValid, "still must be valid deviceType for radioSerial");

      assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "");
      isValid = assetIdRequest.Validate();
      Assert.IsFalse(isValid, "manual/unknown deviceType must have a projectID");

      assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(456661, 0, "");
      isValid = assetIdRequest.Validate();
      Assert.IsTrue(isValid, "Good projectID request");

      assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 6, "AGoodSer ial6");
      isValid = assetIdRequest.Validate();
      Assert.IsTrue(isValid, "Good radioSerial SNM940 request");

      assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(745651, 6, "AGoodSer ial6");
      isValid = assetIdRequest.Validate();
      Assert.IsTrue(isValid, "Good project And radioSerial request");

    }

    [TestMethod]
    public void CanCallAssetIDExecutorWithNonExistingDeviceAsset()
    {
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int)deviceType, deviceSerialNumber);
      Assert.IsTrue(assetIdRequest.Validate(), "AssetIdRequest is invalid");

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void CanCallAssetIDExecutorWithExistingDeviceAsset()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue); ;
      Guid owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int)deviceType, deviceSerialNumber);
      Assert.IsTrue(assetIdRequest.Validate(), "AssetIdRequest is invalid");

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void CanCallAssetIDExecutorWithNonExistingProject()
    {
      int legacyProjectId = new Random().Next(0, int.MaxValue);

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, 0, "");
      Assert.IsTrue(assetIdRequest.Validate(), "AssetIdRequest is invalid");

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void CanCallAssetIDExecutorWithExistingProject()
    {
      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUID = Guid.NewGuid();
      //var serviceTypes = new ServiceTypeMappings();
      var isCreatedOk = CreateAssociation(projectUID, legacyProjectId, customerUID, "Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, 0, "");
      Assert.IsTrue(assetIdRequest.Validate(), "AssetIdRequest is invalid");

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void CanCallAssetIDExecutorWithExistingProjectAndCustomerSub()
    {
      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUID = Guid.NewGuid();
      var isCreatedOk = CreateAssociation(projectUID, legacyProjectId, customerUID, "Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      isCreatedOk = CreateCustomerSub(customerUID, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Customer subscription");

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, 0, "");
      Assert.IsTrue(assetIdRequest.Validate(), "AssetIdRequest is invalid");

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be Man 3d pm (CG==18)");
    }




    private bool CreateAssociation(Guid assetUID, long legacyAssetId, Guid owningCustomerUID, Guid deviceUID, string deviceSerialNumber, string deviceType)
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

    private bool CreateAssociation(Guid projectUID, int legacyProjectId, Guid customerUID, string subToInsert)
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
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2100, 02, 01),
        ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      { CustomerUID = createCustomerEvent.CustomerUID, ProjectUID = createProjectEvent.ProjectUID, LegacyCustomerID = 1234, RelationType = RelationType.Customer, ActionUTC = actionUtc };

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = subToInsert.ToString(),
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUtc
      };

      var associateProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent()
      {
        SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID,
        ProjectUID = createProjectEvent.ProjectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUtc
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      subscriptionContext.StoreEvent(createProjectSubscriptionEvent).Wait();
      var s = subscriptionContext.StoreEvent(associateProjectSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "associateProjectSubscription event not written");
      var g = projectContext.GetProjectBySubcription(createProjectSubscriptionEvent.SubscriptionUID.ToString()); g.Wait();
      return (g.Result != null ? true : false);
    }

    private bool CreateCustomerSub(Guid customerUID, string subToInsert)
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

