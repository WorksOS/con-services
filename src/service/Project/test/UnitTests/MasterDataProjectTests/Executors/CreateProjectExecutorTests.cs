using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.TCCFileAccess;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;
using Xunit;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class CreateProjectExecutorTestsDiFixture : UnitTestsDIFixture<CreateProjectExecutorTestsDiFixture>
  {
    protected static List<TBCPoint> _boundaryLL;
    private static BusinessCenterFile _businessCenterFile;
    private static string _checkBoundaryString;

    private static string _customerUid;

    public CreateProjectExecutorTestsDiFixture()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
      _boundaryLL = new List<TBCPoint>
                    {
        new TBCPoint(-43.5, 172.6),
        new TBCPoint(-43.5003, 172.6),
        new TBCPoint(-43.5003, 172.603),
        new TBCPoint(-43.5, 172.603)
      };

      _checkBoundaryString = "POLYGON((172.6 -43.5,172.6 -43.5003,172.603 -43.5003,172.603 -43.5,172.6 -43.5))";

      _businessCenterFile = new BusinessCenterFile
      {
        FileSpaceId = "u3bdc38d-1afe-470e-8c1c-fc241d4c5e01",
        Path = "/BC Data/Sites/Chch Test Site",
        Name = "CTCTSITECAL.dc",
        CreatedUtc = DateTime.UtcNow.AddDays(-0.5)
      };

      _customerUid = Guid.NewGuid().ToString();
    }

    [Fact]
    public async Task CreateProjectV5TBCExecutor_GetTCCFile()
    {
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var fileRepo = new Mock<IFileRepository>();

      fileRepo.Setup(fr => fr.FolderExists(It.IsAny<string>(), It.IsAny<string>()))
              .ReturnsAsync(true);

      byte[] buffer = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 };

      fileRepo.Setup(fr => fr.GetFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
              .ReturnsAsync(new MemoryStream(buffer));

      var coordinateSystemFileContent = await TccHelper.GetFileContentFromTcc(_businessCenterFile, Log, serviceExceptionHandler, fileRepo.Object);

      Assert.True(buffer.SequenceEqual(coordinateSystemFileContent), "CoordinateSystemFileContent not read from DC.");
    }

    [Fact]
    public async Task CreateProjectV5TBCExecutor_HappyPath()
    {
      var userId = Guid.NewGuid().ToString();
      var customHeaders = new Dictionary<string, string>();

      var request = CreateProjectV5Request.CreateACreateProjectV5Request
      (ProjectType.Standard, new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "projectName",
        "New Zealand Standard Time", _boundaryLL, _businessCenterFile);
      var createProjectEvent = MapV5Models.MapCreateProjectV5RequestToEvent(request, _customerUid);
      Assert.Equal(_checkBoundaryString, createProjectEvent.ProjectBoundary);
      var coordSystemFileContent = "Some dummy content";
      createProjectEvent.CoordinateSystemFileContent = System.Text.Encoding.ASCII.GetBytes(coordSystemFileContent);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
     
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateProjectEvent>())).ReturnsAsync(1);
      projectRepo.Setup(pr => pr.GetProjectOnly(It.IsAny<string>()))
        .ReturnsAsync(new ProjectDatabaseModel { ShortRaptorProjectId = 999 });
      projectRepo.Setup(pr =>
          pr.DoesPolygonOverlap(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(false);

      var createProjectResponseModel = new CreateProjectResponseModel() { Id = "560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97" };
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(pr => pr.CreateProject(It.IsAny<CreateProjectRequestModel>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(createProjectResponseModel);

      var createFileResponseModel = new CreateFileResponseModel
      { FileSpaceId = "2c171c20-ca7a-45d9-a6d6-744ac39adf9b", UploadUrl="an upload url" };     
      var cwsDesignClient = new Mock<ICwsDesignClient>();
      cwsDesignClient.Setup(d => d.CreateAndUploadFile(It.IsAny<Guid>(), It.IsAny<CreateFileRequestModel>(), It.IsAny<Stream>(), customHeaders))
        .ReturnsAsync(createFileResponseModel);

      var projectConfigurationFileResponseModel = new ProjectConfigurationFileResponseModel
      {
        FileName = "some coord sys file",
        FileDownloadLink = "some download link"
      };
      var cwsProfileSettingsClient = new Mock<ICwsProfileSettingsClient>();
      cwsProfileSettingsClient.Setup(ps => ps.GetProjectConfiguration(It.IsAny<Guid>(), ProjectConfigurationFileType.CALIBRATION, customHeaders))
        .ReturnsAsync((ProjectConfigurationFileResponseModel)null);
      cwsProfileSettingsClient.Setup(ps => ps.SaveProjectConfiguration(It.IsAny<Guid>(), ProjectConfigurationFileType.CALIBRATION, It.IsAny<ProjectConfigurationFileRequestModel>(), customHeaders))
        .ReturnsAsync(projectConfigurationFileResponseModel);

      var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
      httpContextAccessor.HttpContext.Request.Path = new PathString("/api/v2/projects");

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

      var executor = RequestExecutorContainerFactory.Build<CreateProjectExecutor>
      (logger, configStore, serviceExceptionHandler, _customerUid, userId, null, customHeaders,
        productivity3dV1ProxyCoord: productivity3dV1ProxyCoord.Object,
        projectRepo: projectRepo.Object, fileRepo: fileRepo.Object, httpContextAccessor: httpContextAccessor,
        dataOceanClient: dataOceanClient.Object, authn: authn.Object,
        cwsProjectClient: cwsProjectClient.Object, cwsDesignClient: cwsDesignClient.Object, 
        cwsProfileSettingsClient: cwsProfileSettingsClient.Object);
      await executor.ProcessAsync(createProjectEvent);
    }

    [Fact]
    public async Task CreateProjectV6Executor_HappyPath()
    {
      var userId = Guid.NewGuid().ToString();
      var customHeaders = new Dictionary<string, string>();

      var coordSystemFileContent = "Some dummy content";
      var request = CreateProjectRequest.CreateACreateProjectRequest
      ( Guid.NewGuid().ToString(),
        ProjectType.Standard, "projectName", "NZ whatsup",
        "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))",
        "some coord file", System.Text.Encoding.ASCII.GetBytes(coordSystemFileContent));
      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(request);
      createProjectEvent.ActionUTC = DateTime.UtcNow;

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(pr => pr.StoreEvent(It.IsAny<CreateProjectEvent>())).ReturnsAsync(1);
       projectRepo.Setup(pr => pr.GetProjectOnly(It.IsAny<string>()))
        .ReturnsAsync(new ProjectDatabaseModel { ShortRaptorProjectId = 999 });
      projectRepo.Setup(pr =>
          pr.DoesPolygonOverlap(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(false);

      var createProjectResponseModel = new CreateProjectResponseModel() { Id = "560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97" };
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(pr => pr.CreateProject(It.IsAny<CreateProjectRequestModel>(), customHeaders)).ReturnsAsync(createProjectResponseModel);

      var createFileResponseModel = new CreateFileResponseModel
      { FileSpaceId = "2c171c20-ca7a-45d9-a6d6-744ac39adf9b", UploadUrl = "an upload url" };
      var cwsDesignClient = new Mock<ICwsDesignClient>();
      cwsDesignClient.Setup(d => d.CreateAndUploadFile(It.IsAny<Guid>(), It.IsAny<CreateFileRequestModel>(), It.IsAny<Stream>(), customHeaders))
        .ReturnsAsync(createFileResponseModel);

      var projectConfigurationFileResponseModel = new ProjectConfigurationFileResponseModel
      {
        FileName = "some coord sys file",
        FileDownloadLink = "some download link"
      };
      var cwsProfileSettingsClient = new Mock<ICwsProfileSettingsClient>();
      cwsProfileSettingsClient.Setup(ps => ps.GetProjectConfiguration(It.IsAny<Guid>(), ProjectConfigurationFileType.CALIBRATION, customHeaders))
        .ReturnsAsync((ProjectConfigurationFileResponseModel)null);
      cwsProfileSettingsClient.Setup(ps => ps.SaveProjectConfiguration(It.IsAny<Guid>(), ProjectConfigurationFileType.CALIBRATION, It.IsAny<ProjectConfigurationFileRequestModel>(), customHeaders))
        .ReturnsAsync(projectConfigurationFileResponseModel);

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

      var executor = RequestExecutorContainerFactory.Build<CreateProjectExecutor>
      (logger, configStore, serviceExceptionHandler, _customerUid, userId, null, customHeaders,
        productivity3dV1ProxyCoord: productivity3dV1ProxyCoord.Object,
        projectRepo: projectRepo.Object, fileRepo: fileRepo.Object, httpContextAccessor: httpContextAccessor,
        dataOceanClient: dataOceanClient.Object, authn: authn.Object,
        cwsProjectClient: cwsProjectClient.Object, cwsDesignClient: cwsDesignClient.Object,
        cwsProfileSettingsClient: cwsProfileSettingsClient.Object);
      await executor.ProcessAsync(createProjectEvent);
    }
  }
}
