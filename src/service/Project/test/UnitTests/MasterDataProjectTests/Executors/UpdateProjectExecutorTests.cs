using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;
using Xunit;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class UpdateProjectExecutorTestsDiFixture : UnitTestsDIFixture<UpdateProjectExecutorTestsDiFixture>
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

    public UpdateProjectExecutorTestsDiFixture()
    {
      try
      {
        AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
      }
      catch (Exception ex)
      {
        Assert.NotNull(ex);
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

    [Fact]
    public async Task UpdateProjectExecutor_HappyPath()
    {
      _configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      _logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      _serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectUid = Guid.NewGuid();
      var projectType = ProjectType.Standard;
      var existingProject = await CreateProject(projectUid, projectType);

      if (existingProject.ProjectUID != null)
      {
        var updateProjectRequest = UpdateProjectRequest.CreateUpdateProjectRequest
        (projectUid, existingProject.ProjectType, existingProject.Name, existingProject.Description,
          existingProject.EndDate,
          null, null,
          _updatedBoundaryString);
        var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(updateProjectRequest);
        updateProjectEvent.ActionUTC = updateProjectEvent.ReceivedUTC = DateTime.UtcNow;

        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateProjectEvent>())).ReturnsAsync(1);
        projectRepo.Setup(pr => pr.GetProject(It.IsAny<string>()))
          .ReturnsAsync(existingProject);

        var projectGeofence = new List<ProjectGeofence>
                              {
          new ProjectGeofence
          {
            GeofenceType = GeofenceType.Project,
            GeofenceUID = _geofenceUid.ToString(),
            ProjectUID = updateProjectRequest.ProjectUid.ToString()
          }
        };
        projectRepo.Setup(pr => pr.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(projectGeofence);

        var subscriptionRepo = new Mock<ISubscriptionRepository>();
        var productivity3dV1ProxyCoord = new Mock<IProductivity3dV1ProxyCoord>();
        productivity3dV1ProxyCoord.Setup(p =>
            p.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
          .ReturnsAsync(new CoordinateSystemSettingsResult());

        var updateExecutor = RequestExecutorContainerFactory.Build<UpdateProjectExecutor>
        (_logger, _configStore, _serviceExceptionHandler,
          _customerUid, _userId, null, _customHeaders,
          _producer.Object, KafkaTopicName,
          productivity3dV1ProxyCoord: productivity3dV1ProxyCoord.Object,
          projectRepo: projectRepo.Object, subscriptionRepo: subscriptionRepo.Object);
        await updateExecutor.ProcessAsync(updateProjectEvent);
      }
    }

    [Fact]
    public async Task UpdateProjectExecutor_ChangeTypeToLandfill_Invalid_NoCoordinateSystem()
    {
      _configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      _logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      _serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectUid = Guid.NewGuid();
      var projectType = ProjectType.Standard;
      var existingProject = await CreateProject(projectUid, projectType);

      if (existingProject.ProjectUID != null)
      {
        var updateProjectRequest = UpdateProjectRequest.CreateUpdateProjectRequest
        (projectUid, ProjectType.LandFill, existingProject.Name,
          existingProject.Description,
          existingProject.EndDate,
          null, null,
          _updatedBoundaryString);
        var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(updateProjectRequest);
        updateProjectEvent.ActionUTC = updateProjectEvent.ReceivedUTC = DateTime.UtcNow;

        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateProjectEvent>())).ReturnsAsync(1);
        projectRepo.Setup(pr => pr.GetProject(It.IsAny<string>()))
          .ReturnsAsync(existingProject);

        var projectGeofence = new List<ProjectGeofence>
                              {
          new ProjectGeofence
          {
            GeofenceType = GeofenceType.Project,
            GeofenceUID = _geofenceUid.ToString(),
            ProjectUID = updateProjectRequest.ProjectUid.ToString()
          }
        };
        projectRepo.Setup(pr => pr.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(projectGeofence);

        var subscriptionRepo = new Mock<ISubscriptionRepository>();
        var productivity3dV1ProxyCoord = new Mock<IProductivity3dV1ProxyCoord>();
        productivity3dV1ProxyCoord.Setup(p =>
            p.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
          .ReturnsAsync(new CoordinateSystemSettingsResult());

        var updateExecutor = RequestExecutorContainerFactory.Build<UpdateProjectExecutor>
        (_logger, _configStore, _serviceExceptionHandler,
          _customerUid, _userId, null, _customHeaders,
          _producer.Object, KafkaTopicName,
          productivity3dV1ProxyCoord: productivity3dV1ProxyCoord.Object,
          projectRepo: projectRepo.Object, subscriptionRepo: subscriptionRepo.Object);
        var ex = await Assert.ThrowsAsync<ServiceException>(async () =>
          await updateExecutor.ProcessAsync(updateProjectEvent));

        var projectErrorCodesProvider = ServiceProvider.GetRequiredService<IErrorCodesProvider>();
        Assert.NotEqual(-1,
          ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(45), StringComparison.Ordinal));
      }
    }

    [Fact]
    public async Task UpdateProjectExecutor_ChangeTypeToLandfill_Invalid_NoSubscription()
    {
      _configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      _logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      _serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectUid = Guid.NewGuid();
      var projectType = ProjectType.Standard;
      var existingProject = await CreateProject(projectUid, projectType, "TheCoordSysFile.dc", new byte[] { 0, 1, 2, 3, 4 });

      if (existingProject.ProjectUID != null)
      {
        var updateProjectRequest = UpdateProjectRequest.CreateUpdateProjectRequest
        (projectUid, ProjectType.LandFill, existingProject.Name,
          existingProject.Description,
          existingProject.EndDate,
          existingProject.CoordinateSystemFileName, null,
          _updatedBoundaryString);
        var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(updateProjectRequest);
        updateProjectEvent.ActionUTC = updateProjectEvent.ReceivedUTC = DateTime.UtcNow;

        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateProjectEvent>())).ReturnsAsync(1);
        projectRepo.Setup(pr => pr.GetProject(It.IsAny<string>()))
          .ReturnsAsync(existingProject);

        var projectGeofence = new List<ProjectGeofence>
                              {
          new ProjectGeofence
          {
            GeofenceType = GeofenceType.Project,
            GeofenceUID = _geofenceUid.ToString(),
            ProjectUID = updateProjectRequest.ProjectUid.ToString()
          }
        };
        projectRepo.Setup(pr => pr.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(projectGeofence);

        var subscriptionRepo = new Mock<ISubscriptionRepository>();
        var productivity3dV1ProxyCoord = new Mock<IProductivity3dV1ProxyCoord>();
        productivity3dV1ProxyCoord.Setup(p =>
            p.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
          .ReturnsAsync(new CoordinateSystemSettingsResult());

        var updateExecutor = RequestExecutorContainerFactory.Build<UpdateProjectExecutor>
        (_logger, _configStore, _serviceExceptionHandler,
          _customerUid, _userId, null, _customHeaders,
          _producer.Object, KafkaTopicName,
          productivity3dV1ProxyCoord: productivity3dV1ProxyCoord.Object,
          projectRepo: projectRepo.Object, subscriptionRepo: subscriptionRepo.Object);
        var ex = await Assert.ThrowsAsync<ServiceException>(async () =>
          await updateExecutor.ProcessAsync(updateProjectEvent));

        var projectErrorCodesProvider = ServiceProvider.GetRequiredService<IErrorCodesProvider>();
        Assert.NotEqual(-1,
          ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(37), StringComparison.Ordinal));
      }
    }

    [Fact]
    public async Task UpdateProjectExecutor_ChangeTypeToLandfill_HappyPath()
    {
      _configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      _logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      _serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectUid = Guid.NewGuid();
      var projectType = ProjectType.Standard;
      var existingProject = await CreateProject(projectUid, projectType, "TheCoordSysFile.dc", new byte[] { 0, 1, 2, 3, 4 });

      if (existingProject.ProjectUID != null)
      {
        var updateProjectRequest = UpdateProjectRequest.CreateUpdateProjectRequest
        (projectUid, ProjectType.LandFill, existingProject.Name,
          existingProject.Description,
          existingProject.EndDate,
          null, null,
          _updatedBoundaryString);
        var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(updateProjectRequest);
        updateProjectEvent.ActionUTC = updateProjectEvent.ReceivedUTC = DateTime.UtcNow;

        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateProjectEvent>())).ReturnsAsync(1);
        projectRepo.Setup(pr => pr.GetProject(It.IsAny<string>()))
          .ReturnsAsync(existingProject);

        var projectGeofence = new List<ProjectGeofence>
                              {
          new ProjectGeofence
          {
            GeofenceType = GeofenceType.Project,
            GeofenceUID = _geofenceUid.ToString(),
            ProjectUID = updateProjectRequest.ProjectUid.ToString()
          }
        };
        projectRepo.Setup(pr => pr.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(projectGeofence);

        var availSubs = new List<Subscription>
                        {
          new Subscription
          {
            SubscriptionUID = Guid.NewGuid().ToString(),
            ServiceTypeID = (int) ServiceTypeEnum.Landfill,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1)
          }
        };

        var subscriptionRepo = new Mock<ISubscriptionRepository>();
        subscriptionRepo.Setup(sr =>
            sr.GetFreeProjectSubscriptionsByCustomer(It.IsAny<string>(), It.IsAny<DateTime>()))
          .ReturnsAsync(availSubs);
        var productivity3dV1ProxyCoord = new Mock<IProductivity3dV1ProxyCoord>();
        productivity3dV1ProxyCoord.Setup(p =>
            p.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(),
              It.IsAny<Dictionary<string, string>>()))
          .ReturnsAsync(new CoordinateSystemSettingsResult());

        var subscriptionProxy = new Mock<ISubscriptionProxy>();
        subscriptionProxy.Setup(sp =>
            sp.AssociateProjectSubscription(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>()))
          .Returns(Task.FromResult(default(int)));

        var updateExecutor = RequestExecutorContainerFactory.Build<UpdateProjectExecutor>
        (_logger, _configStore, _serviceExceptionHandler,
          _customerUid, _userId, null, _customHeaders,
          _producer.Object, KafkaTopicName,
          productivity3dV1ProxyCoord: productivity3dV1ProxyCoord.Object,
          subscriptionProxy: subscriptionProxy.Object,
          projectRepo: projectRepo.Object, subscriptionRepo: subscriptionRepo.Object);
        await updateExecutor.ProcessAsync(updateProjectEvent);
      }
    }

    [Fact]
    public async Task UpdateProjectExecutor_ChangeTypeToLandfill_HappyPath_CoordSystemProvidedOnUpdate()
    {
      _configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      _logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      _serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectUid = Guid.NewGuid();
      var projectType = ProjectType.Standard;
      var existingProject = await CreateProject(projectUid, projectType);

      if (existingProject.ProjectUID != null)
      {
        var updateProjectRequest = UpdateProjectRequest.CreateUpdateProjectRequest
        (projectUid, ProjectType.LandFill, existingProject.Name,
          existingProject.Description,
          existingProject.EndDate,
          "TheCoordSysFile.dc", new byte[] { 1, 2, 3 },
          _updatedBoundaryString);
        var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(updateProjectRequest);
        updateProjectEvent.ActionUTC = updateProjectEvent.ReceivedUTC = DateTime.UtcNow;

        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateProjectEvent>())).ReturnsAsync(1);
        projectRepo.Setup(pr => pr.GetProject(It.IsAny<string>()))
          .ReturnsAsync(existingProject);

        var projectGeofence = new List<ProjectGeofence>
                              {
          new ProjectGeofence
          {
            GeofenceType = GeofenceType.Project,
            GeofenceUID = _geofenceUid.ToString(),
            ProjectUID = updateProjectRequest.ProjectUid.ToString()
          }
        };
        projectRepo.Setup(pr => pr.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(projectGeofence);

        var availSubs = new List<Subscription>
                        {
          new Subscription
          {
            SubscriptionUID = Guid.NewGuid().ToString(),
            ServiceTypeID = (int) ServiceTypeEnum.Landfill,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1)
          }
        };

        var subscriptionRepo = new Mock<ISubscriptionRepository>();
        subscriptionRepo.Setup(sr =>
            sr.GetFreeProjectSubscriptionsByCustomer(It.IsAny<string>(), It.IsAny<DateTime>()))
          .ReturnsAsync(availSubs);
        var productivity3dV1ProxyCoord = new Mock<IProductivity3dV1ProxyCoord>();
        productivity3dV1ProxyCoord.Setup(p =>
            p.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(),
              It.IsAny<Dictionary<string, string>>()))
          .ReturnsAsync(new CoordinateSystemSettingsResult());
        productivity3dV1ProxyCoord.Setup(p => p.CoordinateSystemPost(It.IsAny<long>(), It.IsAny<byte[]>(), It.IsAny<string>(),
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
        dataOceanClient.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), true)).ReturnsAsync(true);
        dataOceanClient.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>(), true)).ReturnsAsync(true);

        var authn = new Mock<ITPaaSApplicationAuthentication>();
        authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

        var updateExecutor = RequestExecutorContainerFactory.Build<UpdateProjectExecutor>
        (_logger, _configStore, _serviceExceptionHandler, _customerUid, _userId, null, _customHeaders,
          _producer.Object, KafkaTopicName,
          productivity3dV1ProxyCoord: productivity3dV1ProxyCoord.Object,
          subscriptionProxy: subscriptionProxy.Object,
          projectRepo: projectRepo.Object, subscriptionRepo: subscriptionRepo.Object, fileRepo: fileRepo.Object,
          dataOceanClient: dataOceanClient.Object, authn: authn.Object);
        await updateExecutor.ProcessAsync(updateProjectEvent);
      }
    }


    [Fact]
    public async Task UpdateProjectExecutor_ChangeTypeToStandard_Invalid()
    {
      _configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      _logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      _serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectUid = Guid.NewGuid();
      var projectType = ProjectType.ProjectMonitoring;
      var existingProject = await CreateProject(projectUid, projectType);

      if (existingProject.ProjectUID != null)
      {
        var updateProjectRequest = UpdateProjectRequest.CreateUpdateProjectRequest
        (projectUid, ProjectType.Standard, existingProject.Name,
          existingProject.Description,
          existingProject.EndDate,
          existingProject.CoordinateSystemFileName, null,
          _updatedBoundaryString);
        var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(updateProjectRequest);
        updateProjectEvent.ActionUTC = updateProjectEvent.ReceivedUTC = DateTime.UtcNow;

        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateProjectEvent>())).ReturnsAsync(1);
        projectRepo.Setup(pr => pr.GetProject(It.IsAny<string>())).ReturnsAsync(existingProject);

        var projectGeofence = new List<ProjectGeofence>
                              {
          new ProjectGeofence
          {
            GeofenceType = GeofenceType.Project,
            GeofenceUID = _geofenceUid.ToString(),
            ProjectUID = updateProjectRequest.ProjectUid.ToString()
          }
        };
        projectRepo.Setup(pr => pr.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(projectGeofence);

        var subscriptionRepo = new Mock<ISubscriptionRepository>();
        var productivity3dV1ProxyCoord = new Mock<IProductivity3dV1ProxyCoord>();
        productivity3dV1ProxyCoord.Setup(p =>
            p.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
          .ReturnsAsync(new CoordinateSystemSettingsResult());

        var updateExecutor = RequestExecutorContainerFactory.Build<UpdateProjectExecutor>
        (_logger, _configStore, _serviceExceptionHandler,
          _customerUid, _userId, null, _customHeaders,
          _producer.Object, KafkaTopicName,
          productivity3dV1ProxyCoord: productivity3dV1ProxyCoord.Object,
          projectRepo: projectRepo.Object, subscriptionRepo: subscriptionRepo.Object);
        var ex = await Assert.ThrowsAsync<ServiceException>(async () =>
          await updateExecutor.ProcessAsync(updateProjectEvent));

        var projectErrorCodesProvider = ServiceProvider.GetRequiredService<IErrorCodesProvider>();
        Assert.NotEqual(-1,
          ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(85), StringComparison.Ordinal));
      }
    }

    [Fact]
    public async Task UpdateProjectExecutor_ChangeTypeBetweenNonStandard_Invalid()
    {
      _configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      _logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      _serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectUid = Guid.NewGuid();
      var projectType = ProjectType.ProjectMonitoring;
      var existingProject = await CreateProject(projectUid, projectType, "TheCoordSysFile.dc", new byte[] { 0, 1, 2, 3, 4 });

      if (existingProject.ProjectUID != null)
      {
        var updateProjectRequest = UpdateProjectRequest.CreateUpdateProjectRequest
        (projectUid, ProjectType.LandFill, existingProject.Name,
          existingProject.Description,
          existingProject.EndDate,
          existingProject.CoordinateSystemFileName, null,
          _updatedBoundaryString);
        var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(updateProjectRequest);
        updateProjectEvent.ActionUTC = updateProjectEvent.ReceivedUTC = DateTime.UtcNow;

        var projectRepo = new Mock<IProjectRepository>();
        projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<UpdateProjectEvent>())).ReturnsAsync(1);
        projectRepo.Setup(pr => pr.GetProject(It.IsAny<string>())).ReturnsAsync(existingProject);

        var projectGeofence = new List<ProjectGeofence>
                              {
          new ProjectGeofence
          {
            GeofenceType = GeofenceType.Project,
            GeofenceUID = _geofenceUid.ToString(),
            ProjectUID = updateProjectRequest.ProjectUid.ToString()
          }
        };
        projectRepo.Setup(pr => pr.GetAssociatedGeofences(It.IsAny<string>())).ReturnsAsync(projectGeofence);

        var subscriptionRepo = new Mock<ISubscriptionRepository>();
        var productivity3dV1ProxyCoord = new Mock<IProductivity3dV1ProxyCoord>();
        productivity3dV1ProxyCoord.Setup(p =>
            p.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
          .ReturnsAsync(new CoordinateSystemSettingsResult());

        var updateExecutor = RequestExecutorContainerFactory.Build<UpdateProjectExecutor>
        (_logger, _configStore, _serviceExceptionHandler,
          _customerUid, _userId, null, _customHeaders,
          _producer.Object, KafkaTopicName,
          productivity3dV1ProxyCoord: productivity3dV1ProxyCoord.Object,
          projectRepo: projectRepo.Object, subscriptionRepo: subscriptionRepo.Object);
        var ex = await Assert.ThrowsAsync<ServiceException>(async () =>
          await updateExecutor.ProcessAsync(updateProjectEvent));

        var projectErrorCodesProvider = ServiceProvider.GetRequiredService<IErrorCodesProvider>();
        Assert.NotEqual(-1,
          ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(85), StringComparison.Ordinal));
      }
    }

    private async Task<ProjectDatabaseModel> CreateProject(Guid projectUid, ProjectType projectType, string coordinateSystemFileName = null, byte[] coordinateSystemFileContent = null)
    {
      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = projectUid,
        ProjectType = projectType,
        CoordinateSystemFileName = coordinateSystemFileName,
        CoordinateSystemFileContent = coordinateSystemFileContent,
        CustomerUID = Guid.NewGuid(),
        CustomerID = 456,
        ProjectName = "projectName",
        Description = "this is the description",
        ProjectStartDate = new DateTime(2017, 01, 20),
        ProjectEndDate = new DateTime(2017, 02, 15),
        ProjectTimezone = "NZ whatsup",
        ProjectBoundary = _boundaryString,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateProjectEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectCustomer>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<AssociateProjectGeofence>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetProjectOnly(It.IsAny<string>()))
        .ReturnsAsync(new ProjectDatabaseModel { LegacyProjectID = 999 });
      projectRepo.Setup(pr =>
          pr.DoesPolygonOverlap(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
            It.IsAny<string>()))
        .ReturnsAsync(false);
      var subscriptionRepo = new Mock<ISubscriptionRepository>();
      subscriptionRepo.Setup(sr =>
          sr.GetFreeProjectSubscriptionsByCustomer(It.IsAny<string>(), It.IsAny<DateTime>()))
        .ReturnsAsync(new List<Subscription>
                      {
          new Subscription {ServiceTypeID = (int) ServiceTypeEnum.ProjectMonitoring, SubscriptionUID = Guid.NewGuid().ToString()}
        });

      var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
      httpContextAccessor.HttpContext.Request.Path = new PathString("/api/v4/projects");

      var productivity3dV1ProxyCoord = new Mock<IProductivity3dV1ProxyCoord>();
      productivity3dV1ProxyCoord.Setup(p =>
          p.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());
      productivity3dV1ProxyCoord.Setup(p => p.CoordinateSystemPost(It.IsAny<long>(), It.IsAny<byte[]>(), It.IsAny<string>(),
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
      dataOceanClient.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), true)).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(),
        It.IsAny<IDictionary<string, string>>(), true)).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var createExecutor = RequestExecutorContainerFactory.Build<CreateProjectExecutor>
      (_logger, _configStore, _serviceExceptionHandler, _customerUid, _userId, null, _customHeaders,
        _producer.Object, KafkaTopicName,
        productivity3dV1ProxyCoord: productivity3dV1ProxyCoord.Object,
        subscriptionProxy: subscriptionProxy.Object,
        projectRepo: projectRepo.Object, subscriptionRepo: subscriptionRepo.Object, fileRepo: fileRepo.Object, httpContextAccessor: httpContextAccessor,
        dataOceanClient: dataOceanClient.Object, authn: authn.Object);
      await createExecutor.ProcessAsync(createProjectEvent);

      return AutoMapperUtility.Automapper.Map<ProjectDatabaseModel>(createProjectEvent);
    }
  }
}
