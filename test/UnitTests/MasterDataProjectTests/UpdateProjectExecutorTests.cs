using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;
using VSS.TCCFileAccess;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Project.WebAPI.Common.Executors;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class UpdateProjectExecutorTests : ExecutorBaseTests
  {
    private static string _boundaryString;
    private static string _updatedBoundaryString;

    private static string _customerUid;
    private static string _userId;
    private static Dictionary<string, string> _customHeaders;
    private static Guid _geofenceUid;
    private static IConfigurationStore _configStore;
    private static ILoggerFactory _logger;
    private static IServiceExceptionHandler _serviceExceptionHandler;
    private static Mock<IKafka> _producer;


    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      try
      {
        AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
      }
      catch (Exception ex)
      {
        Assert.IsNotNull(ex, $"{ex}");
      }

      _boundaryString = "POLYGON((172.6 -43.5,172.6 -43.5003,172.603 -43.5003,172.603 -43.5,172.6 -43.5))";
      _updatedBoundaryString = "POLYGON((44.6 -3.5,44.6 -3.5003,44.603 -3.5003,44.603 -3.5,44.6 -3.5))";

      _customerUid = Guid.NewGuid().ToString();
      _geofenceUid = Guid.NewGuid();
      _userId = Guid.NewGuid().ToString();
      _customHeaders = new Dictionary<string, string>();

      _producer = new Mock<IKafka>();
      _producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      _producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));
    }

    [TestMethod]
    public async Task UpdateProjectExecutor_HappyPath()
    {
      var createProjectRequest = CreateProjectRequest.CreateACreateProjectRequest
      (Guid.NewGuid(), Guid.NewGuid(),
        ProjectType.Standard, "projectName", "this is the description",
        new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "NZ whatsup",
        _boundaryString, 456, null, null);
      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(createProjectRequest);
      createProjectEvent.ActionUTC = createProjectEvent.ReceivedUTC = DateTime.UtcNow;

      _configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      _logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      _serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      
      await CreateProject(createProjectRequest);

      if (createProjectRequest.ProjectUID != null)
      {
        var updateProjectRequest = UpdateProjectRequest.CreateUpdateProjectRequest
        (createProjectRequest.ProjectUID.Value, createProjectEvent.ProjectType, createProjectRequest.ProjectName, createProjectRequest.Description,
          createProjectEvent.ProjectEndDate,
          createProjectEvent.CoordinateSystemFileName, createProjectEvent.CoordinateSystemFileContent,
          _updatedBoundaryString);
        var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(updateProjectRequest);
        updateProjectEvent.ActionUTC = updateProjectEvent.ReceivedUTC = DateTime.UtcNow;

        var existingProject = AutoMapperUtility.Automapper.Map<Repositories.DBModels.Project>(createProjectEvent);

        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateProjectEvent>())).ReturnsAsync(1);
        projectRepo.Setup(pr => pr.GetProject(It.IsAny<string>()))
          .ReturnsAsync(existingProject);

        var projectGeofence = new List<ProjectGeofence>()
        {
          new ProjectGeofence()
          {
            GeofenceType = GeofenceType.Project,
            GeofenceUID = _geofenceUid.ToString(),
            ProjectUID = updateProjectRequest.ProjectUid.ToString()
          }
        };
        projectRepo.Setup(pr => pr.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(projectGeofence);

        var geofence = new GeofenceData()
        {
          GeofenceUID = _geofenceUid,
          GeofenceType = GeofenceType.Project.ToString(),
          GeometryWKT = updateProjectEvent.ProjectBoundary
        };

        var geofenceProxy = new Mock<IGeofenceProxy>();
        geofenceProxy.Setup(gp => gp.GetGeofenceForCustomer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(geofence);
        geofenceProxy.Setup(gp => gp.UpdateGeofence(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Guid>(),
            It.IsAny<double>(), It.IsAny<Dictionary<string, string>>()))
          .ReturnsAsync(_geofenceUid);

        var subscriptionRepo = new Mock<ISubscriptionRepository>();
        var raptorProxy = new Mock<IRaptorProxy>();
        raptorProxy.Setup(rp =>
            rp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
          .ReturnsAsync(new CoordinateSystemSettingsResult());

        var updateExecutor = RequestExecutorContainerFactory.Build<UpdateProjectExecutor>
        (_logger, _configStore, _serviceExceptionHandler,
          _customerUid, _userId, null, _customHeaders,
          _producer.Object, KafkaTopicName,
          geofenceProxy.Object, raptorProxy.Object, null,
          projectRepo.Object, subscriptionRepo.Object, null, null, null);
        await updateExecutor.ProcessAsync(updateProjectEvent);
      }
    }

    [TestMethod]
    private async Task CreateProject(CreateProjectRequest createProjectRequest)
    {
      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(createProjectRequest);
      createProjectEvent.ActionUTC = createProjectEvent.ReceivedUTC = DateTime.UtcNow;

      //var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      //var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      //var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      //var producer = new Mock<IKafka>();
      //producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      //producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateProjectEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectCustomer>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetProjectOnly(It.IsAny<string>()))
        .ReturnsAsync(new Repositories.DBModels.Project() { LegacyProjectID = 999 });
      projectRepo.Setup(pr =>
          pr.DoesPolygonOverlap(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
        .ReturnsAsync(false);
      var subscriptionRepo = new Mock<ISubscriptionRepository>();
      subscriptionRepo.Setup(sr =>
          sr.GetFreeProjectSubscriptionsByCustomer(It.IsAny<string>(), It.IsAny<DateTime>()))
        .ReturnsAsync(new List<Subscription>()
        {
          new Subscription()
            {ServiceTypeID = (int) ServiceTypeEnum.ProjectMonitoring, SubscriptionUID = Guid.NewGuid().ToString()}
        });

      var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
      httpContextAccessor.HttpContext.Request.Path = new PathString("/api/v4/projects");

      var geofenceProxy = new Mock<IGeofenceProxy>();
      geofenceProxy.Setup(gp => gp.CreateGeofence(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Guid>(),
          It.IsAny<double>(), It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(_geofenceUid);

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

      var createExecutor = RequestExecutorContainerFactory.Build<CreateProjectExecutor>
      (_logger, _configStore, _serviceExceptionHandler,
        _customerUid, _userId, null, _customHeaders,
        _producer.Object, KafkaTopicName,
        geofenceProxy.Object, raptorProxy.Object, subscriptionProxy.Object,
        projectRepo.Object, subscriptionRepo.Object, fileRepo.Object, null, httpContextAccessor);
      await createExecutor.ProcessAsync(createProjectEvent);

    }

  }
}
