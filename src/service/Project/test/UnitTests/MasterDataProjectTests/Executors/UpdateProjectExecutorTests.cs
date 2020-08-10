using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.DataOcean.Client;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;
using Xunit;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class UpdateProjectExecutorTests : UnitTestsDIFixture<UpdateProjectExecutorTests>
  {
    private static string _boundaryString;
    private static string _updatedBoundaryString;

    public UpdateProjectExecutorTests()
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
    }

    [Fact]
    public async Task UpdateProjectExecutor_HappyPath()
    {
      var projectType = CwsProjectType.AcceptsTagFiles;
      var existingProject = await CreateProject(_projectUid.ToString(), projectType);

      if (existingProject.ProjectUID != null)
      {
        var updatedCoordSysFileContent = System.Text.Encoding.ASCII.GetBytes("Some other dummy content");
        var updateProjectRequest = UpdateProjectRequest.CreateUpdateProjectRequest
        (_projectUid, existingProject.ProjectType, existingProject.Name, "updated dummy coord system", updatedCoordSysFileContent,
          _updatedBoundaryString);
        var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(updateProjectRequest);
        updateProjectEvent.ActionUTC = DateTime.UtcNow;

        var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
        var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
        var cwsProjectClient = new Mock<ICwsProjectClient>();
        cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);
        cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

        var createFileResponseModel = new CreateFileResponseModel
          { FileSpaceId = "2c171c20-ca7a-45d9-a6d6-744ac39adf9b", UploadUrl = "an upload url" };
        var cwsDesignClient = new Mock<ICwsDesignClient>();
        cwsDesignClient.Setup(d => d.CreateAndUploadFile(It.IsAny<Guid>(), It.IsAny<CreateFileRequestModel>(), It.IsAny<Stream>(), _customHeaders))
          .ReturnsAsync(createFileResponseModel);

        var projectConfigurationModel = new ProjectConfigurationModel
        {
          FileName = "some coord sys file",
          FileDownloadLink = "some download link"
        };
        var updatedConfigurationModel = new ProjectConfigurationModel
        {
          FileName = "updated coord sys file",
          FileDownloadLink = "updated download link"
        };
        var cwsProfileSettingsClient = new Mock<ICwsProfileSettingsClient>();
        cwsProfileSettingsClient.Setup(ps => ps.GetProjectConfiguration(It.IsAny<Guid>(), ProjectConfigurationFileType.CALIBRATION, _customHeaders))
          .ReturnsAsync(projectConfigurationModel);
        cwsProfileSettingsClient.Setup(ps => ps.UpdateProjectConfiguration(It.IsAny<Guid>(), ProjectConfigurationFileType.CALIBRATION, It.IsAny<ProjectConfigurationFileRequestModel>(), _customHeaders))
          .ReturnsAsync(updatedConfigurationModel);

        var productivity3dV1ProxyCoord = new Mock<IProductivity3dV1ProxyCoord>();
        productivity3dV1ProxyCoord.Setup(p =>
            p.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<HeaderDictionary>()))
          .ReturnsAsync(new CoordinateSystemSettingsResult());
        productivity3dV1ProxyCoord.Setup(p => p.CoordinateSystemPost(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<string>(),
            It.IsAny<HeaderDictionary>()))
          .ReturnsAsync(new CoordinateSystemSettingsResult());

        var dataOceanClient = new Mock<IDataOceanClient>();
        dataOceanClient.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(true);
        dataOceanClient.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(),
          It.IsAny<HeaderDictionary>())).ReturnsAsync(true);

        var authn = new Mock<ITPaaSApplicationAuthentication>();
        authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

        var updateExecutor = RequestExecutorContainerFactory.Build<UpdateProjectExecutor>
        (_loggerFactory, _configStore, ServiceExceptionHandler,
          _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
          productivity3dV1ProxyCoord.Object, dataOceanClient: dataOceanClient.Object, authn: authn.Object,
          cwsProjectClient: cwsProjectClient.Object, cwsDesignClient: cwsDesignClient.Object,
          cwsProfileSettingsClient: cwsProfileSettingsClient.Object);
        await updateExecutor.ProcessAsync(updateProjectEvent);
      }
    }

    private async Task<ProjectDatabaseModel> CreateProject(string projectUid, CwsProjectType projectType)
    {
      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = new Guid(projectUid),
        ProjectType = projectType,
        CoordinateSystemFileName = string.Empty,
        CoordinateSystemFileContent = null,
        CustomerUID = _customerUid,
        ProjectName = "projectName",
        ProjectTimezone = "NZ whatsup",
        ProjectBoundary = _boundaryString,
        ActionUTC = DateTime.UtcNow
      };

      var createProjectResponseModel = new CreateProjectResponseModel() { TRN = _projectTrn };
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(pr => pr.CreateProject(It.IsAny<CreateProjectRequestModel>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(createProjectResponseModel);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
      httpContextAccessor.HttpContext.Request.Path = new PathString("/api/v6/projects");

      var productivity3dV1ProxyCoord = new Mock<IProductivity3dV1ProxyCoord>();
      productivity3dV1ProxyCoord.Setup(p =>
          p.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<HeaderDictionary>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());
      productivity3dV1ProxyCoord.Setup(p => p.CoordinateSystemPost(It.IsAny<Guid>(), It.IsAny<byte[]>(), It.IsAny<string>(),
          It.IsAny<HeaderDictionary>()))
        .ReturnsAsync(new CoordinateSystemSettingsResult());

     var dataOceanClient = new Mock<IDataOceanClient>();
      dataOceanClient.Setup(f => f.FolderExists(It.IsAny<string>(), It.IsAny<HeaderDictionary>())).ReturnsAsync(true);
      dataOceanClient.Setup(f => f.PutFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(),
        It.IsAny<HeaderDictionary>())).ReturnsAsync(true);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var createExecutor = RequestExecutorContainerFactory.Build<CreateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler, _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
         productivity3dV1ProxyCoord.Object, httpContextAccessor: httpContextAccessor,
        dataOceanClient: dataOceanClient.Object, authn: authn.Object,
        cwsProjectClient: cwsProjectClient.Object);
      await createExecutor.ProcessAsync(createProjectEvent);

      return AutoMapperUtility.Automapper.Map<ProjectDatabaseModel>(createProjectEvent);
    }
  }
}
