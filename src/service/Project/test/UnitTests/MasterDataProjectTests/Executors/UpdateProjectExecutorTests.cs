using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.TCCFileAccess;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
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
    }

    [Fact]
    public async Task UpdateProjectExecutor_HappyPath()
    {
      _configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      _logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      _serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectUid = Guid.NewGuid();
      var projectType = ProjectType.Standard;
      var existingProject = await CreateProject(projectUid.ToString(), projectType);

      if (existingProject.ProjectUID != null)
      {
        var updateProjectRequest = UpdateProjectRequest.CreateUpdateProjectRequest
        (projectUid, existingProject.ProjectType, existingProject.Name, existingProject.Description,
          existingProject.EndDate,
          null, null,
          _updatedBoundaryString);
        var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(updateProjectRequest);
        updateProjectEvent.ActionUTC = DateTime.UtcNow;

        // CCSSSCON-214 need to send update to cws only if boundary/name changed
        //var createProjectResponseModel = new CreateProjectResponseModel() { Id = "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97" };
        var projectClient = new Mock<ICwsProjectClient>();
        //projectClient.Setup(pr => pr.CreateProject(It.IsAny<CreateProjectRequestModel>(), null)).ReturnsAsync(createProjectResponseModel);

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

        var productivity3dV1ProxyCoord = new Mock<IProductivity3dV1ProxyCoord>();
        productivity3dV1ProxyCoord.Setup(p =>
            p.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
          .ReturnsAsync(new CoordinateSystemSettingsResult());

        var updateExecutor = RequestExecutorContainerFactory.Build<UpdateProjectExecutor>
        (_logger, _configStore, _serviceExceptionHandler,
          _customerUid, _userId, null, _customHeaders,
          productivity3dV1ProxyCoord: productivity3dV1ProxyCoord.Object,
          projectRepo: projectRepo.Object, cwsProjectClient: projectClient.Object);
        await updateExecutor.ProcessAsync(updateProjectEvent);
      }
    }
  
    private async Task<ProjectDatabaseModel> CreateProject(string projectUid, ProjectType projectType, string coordinateSystemFileName = null, byte[] coordinateSystemFileContent = null)
    {
      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = new Guid(projectUid),
        ProjectType = projectType,
        CoordinateSystemFileName = coordinateSystemFileName,
        CoordinateSystemFileContent = coordinateSystemFileContent,
        CustomerUID = Guid.NewGuid(),
        ProjectName = "projectName",
        Description = "this is the description",
        ProjectStartDate = new DateTime(2017, 01, 20),
        ProjectEndDate = new DateTime(2017, 02, 15),
        ProjectTimezone = "NZ whatsup",
        ProjectBoundary = _boundaryString,
        ActionUTC = DateTime.UtcNow
      };

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateProjectEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetProjectOnly(It.IsAny<string>()))
        .ReturnsAsync(new ProjectDatabaseModel { ShortRaptorProjectId = 999 });
      projectRepo.Setup(pr =>
          pr.DoesPolygonOverlap(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
            It.IsAny<string>()))
        .ReturnsAsync(false);

      var createProjectResponseModel = new CreateProjectResponseModel() { Id = "560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97" };
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(pr => pr.CreateProject(It.IsAny<CreateProjectRequestModel>(), null)).ReturnsAsync(createProjectResponseModel);

      var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
      httpContextAccessor.HttpContext.Request.Path = new PathString("/api/v6/projects");

      var productivity3dV1ProxyCoord = new Mock<IProductivity3dV1ProxyCoord>();
      productivity3dV1ProxyCoord.Setup(p =>
          p.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());
      productivity3dV1ProxyCoord.Setup(p => p.CoordinateSystemPost(It.IsAny<long>(), It.IsAny<byte[]>(), It.IsAny<string>(),
          It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());

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

      var createExecutor = RequestExecutorContainerFactory.Build<CreateProjectExecutor>
      (_logger, _configStore, _serviceExceptionHandler, _customerUid, _userId, null, _customHeaders,
        productivity3dV1ProxyCoord: productivity3dV1ProxyCoord.Object,
        projectRepo: projectRepo.Object, fileRepo: fileRepo.Object, httpContextAccessor: httpContextAccessor,
        dataOceanClient: dataOceanClient.Object, authn: authn.Object,
        cwsProjectClient: cwsProjectClient.Object);
      await createExecutor.ProcessAsync(createProjectEvent);

      return AutoMapperUtility.Automapper.Map<ProjectDatabaseModel>(createProjectEvent);
    }
  }
}
