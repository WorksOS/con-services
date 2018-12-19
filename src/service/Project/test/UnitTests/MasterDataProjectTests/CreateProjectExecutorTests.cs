using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class CreateProjectExecutorTests : ExecutorBaseTests
  {
    protected static List<TBCPoint> _boundaryLL;
    private static BusinessCenterFile _businessCenterFile;
    private static string _checkBoundaryString;

    private static string _customerUid;

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
      _boundaryLL = new List<TBCPoint>()
      {
        new TBCPoint(-43.5, 172.6),
        new TBCPoint(-43.5003, 172.6),
        new TBCPoint(-43.5003, 172.603),
        new TBCPoint(-43.5, 172.603)
      };

      _checkBoundaryString = "POLYGON((172.6 -43.5,172.6 -43.5003,172.603 -43.5003,172.603 -43.5,172.6 -43.5))";

      _businessCenterFile = new BusinessCenterFile()
      {
        FileSpaceId = "u3bdc38d-1afe-470e-8c1c-fc241d4c5e01",
        Path = "/BC Data/Sites/Chch Test Site",
        Name = "CTCTSITECAL.dc",
        CreatedUtc = DateTime.UtcNow.AddDays(-0.5)
      };

      _customerUid = Guid.NewGuid().ToString();
    }


    [TestMethod]
    public async Task CreateProjectV2Executor_GetTCCFile()
    {
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var fileRepo = new Mock<IFileRepository>();
      fileRepo.Setup(fr => fr.FolderExists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
      byte[] buffer = {1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3};
      fileRepo.Setup(fr => fr.GetFile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new MemoryStream(buffer));

      var coordinateSystemFileContent = await TccHelper.GetFileContentFromTcc(_businessCenterFile,
          logger.CreateLogger<CreateProjectExecutorTests>(), serviceExceptionHandler, fileRepo.Object)
        .ConfigureAwait(false);
      Assert.IsTrue(buffer.SequenceEqual(coordinateSystemFileContent), "CoordinateSystemFileContent not read from DC.");
    }

    [TestMethod]
    public async Task CreateProjectV2Executor_HappyPath()
    {
      var userId = Guid.NewGuid().ToString();
      var customHeaders = new Dictionary<string, string>();

      var request = CreateProjectV2Request.CreateACreateProjectV2Request
      (ProjectType.ProjectMonitoring, new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "projectName",
        "New Zealand Standard Time", _boundaryLL, _businessCenterFile);
      var createProjectEvent = MapV2Models.MapCreateProjectV2RequestToEvent(request, _customerUid);
      Assert.AreEqual(_checkBoundaryString, createProjectEvent.ProjectBoundary, "Invalid ProjectBoundary in WKT");
      var coordSystemFileContent = "Some dummy content";
      createProjectEvent.CoordinateSystemFileContent = System.Text.Encoding.ASCII.GetBytes(coordSystemFileContent);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateProjectEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectCustomer>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetProjectOnly(It.IsAny<string>()))
        .ReturnsAsync(new Repositories.DBModels.Project() {LegacyProjectID = 999});
      projectRepo.Setup(pr =>
          pr.DoesPolygonOverlap(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
            It.IsAny<string>()))
        .ReturnsAsync(false);
      var subscriptionRepo = new Mock<ISubscriptionRepository>();
      subscriptionRepo.Setup(sr =>
          sr.GetFreeProjectSubscriptionsByCustomer(It.IsAny<string>(), It.IsAny<DateTime>()))
        .ReturnsAsync(new List<Subscription>()
        {
          new Subscription()
            {ServiceTypeID = (int) ServiceTypeEnum.ProjectMonitoring, SubscriptionUID = Guid.NewGuid().ToString()}
        });

      var httpContextAccessor = new HttpContextAccessor {HttpContext = new DefaultHttpContext()};
      httpContextAccessor.HttpContext.Request.Path = new PathString("/api/v2/projects");

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(rp =>
          rp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());
      raptorProxy.Setup(rp => rp.CoordinateSystemPost(It.IsAny<long>(), It.IsAny<byte[]>(), It.IsAny<string>(),
          It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());
      var subscriptionProxy = new Mock<ISubscriptionProxy>();
      subscriptionProxy.Setup(sp =>
          sp.AssociateProjectSubscription(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>()))
        .Returns(Task.FromResult(default(int)));

      var fileRepo = new Mock<IFileRepository>();
      fileRepo.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
      fileRepo.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
        It.IsAny<Stream>(), It.IsAny<long>())).ReturnsAsync(true);

      var dataOceanClient = new Mock<IDataOceanClient>();
      dataOceanClient.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), 
        It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var executor = RequestExecutorContainerFactory.Build<CreateProjectExecutor>
      (logger, configStore, serviceExceptionHandler, _customerUid, userId, null, customHeaders,
        producer.Object, KafkaTopicName, raptorProxy.Object, subscriptionProxy.Object, null, null, null,
        projectRepo.Object, subscriptionRepo.Object, fileRepo.Object, null, httpContextAccessor, 
        dataOceanClient.Object, null, authn.Object);
      await executor.ProcessAsync(createProjectEvent);
    }

    [TestMethod]
    public async Task CreateProjectV4Executor_HappyPath()
    {
      var userId = Guid.NewGuid().ToString();
      var customHeaders = new Dictionary<string, string>();
      var geofenceUid = Guid.NewGuid();

      var request = CreateProjectRequest.CreateACreateProjectRequest
      (Guid.NewGuid(), Guid.NewGuid(),
        ProjectType.Standard, "projectName", "this is the description",
        new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "NZ whatsup",
        "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))",
        456, null, null);
      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(request);
      createProjectEvent.ActionUTC = createProjectEvent.ReceivedUTC = DateTime.UtcNow;

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateProjectEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectCustomer>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetProjectOnly(It.IsAny<string>()))
        .ReturnsAsync(new Repositories.DBModels.Project() {LegacyProjectID = 999});
      projectRepo.Setup(pr =>
          pr.DoesPolygonOverlap(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
            It.IsAny<string>()))
        .ReturnsAsync(false);
      var subscriptionRepo = new Mock<ISubscriptionRepository>();
      subscriptionRepo.Setup(sr =>
          sr.GetFreeProjectSubscriptionsByCustomer(It.IsAny<string>(), It.IsAny<DateTime>()))
        .ReturnsAsync(new List<Subscription>()
        {
          new Subscription()
            {ServiceTypeID = (int) ServiceTypeEnum.ProjectMonitoring, SubscriptionUID = Guid.NewGuid().ToString()}
        });

      var httpContextAccessor = new HttpContextAccessor {HttpContext = new DefaultHttpContext()};
      httpContextAccessor.HttpContext.Request.Path = new PathString("/api/v4/projects");

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(rp =>
          rp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());
      raptorProxy.Setup(rp => rp.CoordinateSystemPost(It.IsAny<long>(), It.IsAny<byte[]>(), It.IsAny<string>(),
          It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());
      var subscriptionProxy = new Mock<ISubscriptionProxy>();
      subscriptionProxy.Setup(sp =>
          sp.AssociateProjectSubscription(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>()))
        .Returns(Task.FromResult(default(int)));

      var fileRepo = new Mock<IFileRepository>();
      fileRepo.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
      fileRepo.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
        It.IsAny<Stream>(), It.IsAny<long>())).ReturnsAsync(true);

      var dataOceanClient = new Mock<IDataOceanClient>();
      dataOceanClient.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(),
        It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var executor = RequestExecutorContainerFactory.Build<CreateProjectExecutor>
      (logger, configStore, serviceExceptionHandler, _customerUid, userId, null, 
        customHeaders, producer.Object, KafkaTopicName, raptorProxy.Object, 
        subscriptionProxy.Object, null, null, null, projectRepo.Object, 
        subscriptionRepo.Object, fileRepo.Object, null, httpContextAccessor,
        dataOceanClient.Object, null, authn.Object);
      await executor.ProcessAsync(createProjectEvent);
    }

    [TestMethod]
    public async Task CreateProjectV4Executor_LandfillProject_MissingCoordSystem()
    {
      var userId = Guid.NewGuid().ToString();
      var customHeaders = new Dictionary<string, string>();
      var geofenceUid = Guid.NewGuid();

      var request = CreateProjectRequest.CreateACreateProjectRequest
      (Guid.NewGuid(), Guid.NewGuid(),
        ProjectType.LandFill, "projectName", "this is the description",
        new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "NZ whatsup",
        "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))",
        456, null, null);
      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(request);
      createProjectEvent.ActionUTC = createProjectEvent.ReceivedUTC = DateTime.UtcNow;

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateProjectEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectCustomer>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetProjectOnly(It.IsAny<string>()))
        .ReturnsAsync(new Repositories.DBModels.Project() {LegacyProjectID = 999});
      projectRepo.Setup(pr =>
          pr.DoesPolygonOverlap(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
            It.IsAny<string>()))
        .ReturnsAsync(false);
      var subscriptionRepo = new Mock<ISubscriptionRepository>();
      subscriptionRepo.Setup(sr =>
          sr.GetFreeProjectSubscriptionsByCustomer(It.IsAny<string>(), It.IsAny<DateTime>()))
        .ReturnsAsync(new List<Subscription>()
        {
          new Subscription()
            {ServiceTypeID = (int) ServiceTypeEnum.Landfill, SubscriptionUID = Guid.NewGuid().ToString()},
        });

      var httpContextAccessor = new HttpContextAccessor {HttpContext = new DefaultHttpContext()};
      httpContextAccessor.HttpContext.Request.Path = new PathString("/api/v4/projects");

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(rp =>
          rp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());
      raptorProxy.Setup(rp => rp.CoordinateSystemPost(It.IsAny<long>(), It.IsAny<byte[]>(), It.IsAny<string>(),
          It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());
      var subscriptionProxy = new Mock<ISubscriptionProxy>();
      subscriptionProxy.Setup(sp =>
          sp.AssociateProjectSubscription(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>()))
        .Returns(Task.FromResult(default(int)));

      var fileRepo = new Mock<IFileRepository>();
      fileRepo.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
      fileRepo.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
        It.IsAny<Stream>(), It.IsAny<long>())).ReturnsAsync(true);

      var dataOceanClient = new Mock<IDataOceanClient>();
      dataOceanClient.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(),
        It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var executor = RequestExecutorContainerFactory.Build<CreateProjectExecutor>
      (logger, configStore, serviceExceptionHandler, _customerUid, userId, null, 
        customHeaders, producer.Object, KafkaTopicName, raptorProxy.Object,
        subscriptionProxy.Object, null, null, null, projectRepo.Object, 
        subscriptionRepo.Object, fileRepo.Object, null, httpContextAccessor, 
        dataOceanClient.Object, null, authn.Object);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () =>
        await executor.ProcessAsync(createProjectEvent));

      var projectErrorCodesProvider = ServiceProvider.GetRequiredService<IErrorCodesProvider>();
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(45)));
    }

    [TestMethod]
    public async Task CreateProjectV4Executor_LandfillProject_MissingLandfillSub()
    {
      var userId = Guid.NewGuid().ToString();
      var customHeaders = new Dictionary<string, string>();
      var geofenceUid = Guid.NewGuid();
      var coordSystemName = "coordsystem.dc";
      byte[] coordSystemFileContent = {1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3};

      var request = CreateProjectRequest.CreateACreateProjectRequest
      (Guid.NewGuid(), Guid.NewGuid(),
        ProjectType.LandFill, "projectName", "this is the description",
        new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "NZ whatsup",
        "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))",
        456, coordSystemName, coordSystemFileContent);
      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(request);
      createProjectEvent.ActionUTC = createProjectEvent.ReceivedUTC = DateTime.UtcNow;

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateProjectEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectCustomer>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetProjectOnly(It.IsAny<string>()))
        .ReturnsAsync(new Repositories.DBModels.Project() {LegacyProjectID = 999});
      projectRepo.Setup(pr =>
          pr.DoesPolygonOverlap(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
            It.IsAny<string>()))
        .ReturnsAsync(false);
      var subscriptionRepo = new Mock<ISubscriptionRepository>();
      subscriptionRepo.Setup(sr =>
          sr.GetFreeProjectSubscriptionsByCustomer(It.IsAny<string>(), It.IsAny<DateTime>()))
        .ReturnsAsync(new List<Subscription>()
        {
          new Subscription()
            {ServiceTypeID = (int) ServiceTypeEnum.ProjectMonitoring, SubscriptionUID = Guid.NewGuid().ToString()},
        });

      var httpContextAccessor = new HttpContextAccessor {HttpContext = new DefaultHttpContext()};
      httpContextAccessor.HttpContext.Request.Path = new PathString("/api/v4/projects");

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(rp =>
          rp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());
      raptorProxy.Setup(rp => rp.CoordinateSystemPost(It.IsAny<long>(), It.IsAny<byte[]>(), It.IsAny<string>(),
          It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());
      var subscriptionProxy = new Mock<ISubscriptionProxy>();
      subscriptionProxy.Setup(sp =>
          sp.AssociateProjectSubscription(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>()))
        .Returns(Task.FromResult(default(int)));

      var fileRepo = new Mock<IFileRepository>();
      fileRepo.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
      fileRepo.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
        It.IsAny<Stream>(), It.IsAny<long>())).ReturnsAsync(true);

      var dataOceanClient = new Mock<IDataOceanClient>();
      dataOceanClient.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(),
        It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var executor = RequestExecutorContainerFactory.Build<CreateProjectExecutor>
      (logger, configStore, serviceExceptionHandler, _customerUid, userId, null, 
        customHeaders, producer.Object, KafkaTopicName, raptorProxy.Object, 
        subscriptionProxy.Object, null, null, null, projectRepo.Object, 
        subscriptionRepo.Object, fileRepo.Object, null, httpContextAccessor,
        dataOceanClient.Object, null, authn.Object);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () =>
        await executor.ProcessAsync(createProjectEvent));

      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(ProjectErrorCodesProvider.FirstNameWithOffset(37)));
    }

    [TestMethod]
    public async Task CreateProjectV4Executor_LandfillProject_HappyPath()
    {
      var userId = Guid.NewGuid().ToString();
      var customHeaders = new Dictionary<string, string>();
      var geofenceUid = Guid.NewGuid();
      var coordSystemName = "coordsystem.dc";
      byte[] coordSystemFileContent = {1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3};

      var request = CreateProjectRequest.CreateACreateProjectRequest
      (Guid.NewGuid(), Guid.NewGuid(),
        ProjectType.LandFill, "projectName", "this is the description",
        new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "NZ whatsup",
        "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))",
        456, coordSystemName, coordSystemFileContent);
      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(request);
      createProjectEvent.ActionUTC = createProjectEvent.ReceivedUTC = DateTime.UtcNow;

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateProjectEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectCustomer>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetProjectOnly(It.IsAny<string>()))
        .ReturnsAsync(new Repositories.DBModels.Project() {LegacyProjectID = 999});
      projectRepo.Setup(pr =>
          pr.DoesPolygonOverlap(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
            It.IsAny<string>()))
        .ReturnsAsync(false);
      var subscriptionRepo = new Mock<ISubscriptionRepository>();
      subscriptionRepo.Setup(sr =>
          sr.GetFreeProjectSubscriptionsByCustomer(It.IsAny<string>(), It.IsAny<DateTime>()))
        .ReturnsAsync(new List<Subscription>()
        {
          new Subscription()
            {ServiceTypeID = (int) ServiceTypeEnum.Landfill, SubscriptionUID = Guid.NewGuid().ToString()},
        });

      var httpContextAccessor = new HttpContextAccessor {HttpContext = new DefaultHttpContext()};
      httpContextAccessor.HttpContext.Request.Path = new PathString("/api/v4/projects");

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(rp =>
          rp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());
      raptorProxy.Setup(rp => rp.CoordinateSystemPost(It.IsAny<long>(), It.IsAny<byte[]>(), It.IsAny<string>(),
          It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());
      var subscriptionProxy = new Mock<ISubscriptionProxy>();
      subscriptionProxy.Setup(sp =>
          sp.AssociateProjectSubscription(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>()))
        .Returns(Task.FromResult(default(int)));

      var fileRepo = new Mock<IFileRepository>();
      fileRepo.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
      fileRepo.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
        It.IsAny<Stream>(), It.IsAny<long>())).ReturnsAsync(true);

      var dataOceanClient = new Mock<IDataOceanClient>();
      dataOceanClient.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(),
        It.IsAny<IDictionary<string, string>>())).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var executor = RequestExecutorContainerFactory.Build<CreateProjectExecutor>
      (logger, configStore, serviceExceptionHandler, _customerUid, userId, null, customHeaders,
        producer.Object, KafkaTopicName, raptorProxy.Object, subscriptionProxy.Object, null, null, null,
        projectRepo.Object, subscriptionRepo.Object, fileRepo.Object, null, httpContextAccessor, 
        dataOceanClient.Object, null, authn.Object);
      await executor.ProcessAsync(createProjectEvent);
    }
  }
}
