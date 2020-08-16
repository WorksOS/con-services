using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.TCCFileAccess;
using VSS.WebApi.Common;
using Xunit;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class CreateProjectTBCExecutorTests : UnitTestsDIFixture<CreateProjectTBCExecutorTests>
  {
    private static List<TBCPoint> _boundaryLL;
    private static BusinessCenterFile _businessCenterFile;
    private static string _checkBoundaryString;

    public CreateProjectTBCExecutorTests()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
      _boundaryLL = new List<TBCPoint> {new TBCPoint(-43.5, 172.6), new TBCPoint(-43.5003, 172.6), new TBCPoint(-43.5003, 172.603), new TBCPoint(-43.5, 172.603)};

      _checkBoundaryString = "POLYGON((172.6 -43.5,172.6 -43.5003,172.603 -43.5003,172.603 -43.5,172.6 -43.5))";
      _businessCenterFile = new BusinessCenterFile {FileSpaceId = "u3bdc38d-1afe-470e-8c1c-fc241d4c5e01", Path = "/BC Data/Sites/Chch Test Site", Name = "CTCTSITECAL.dc", CreatedUtc = DateTime.UtcNow.AddDays(-0.5)};
    }

    [Fact]
    public async Task CreateProjectV5TBCExecutor_GetTCCFile()
    {
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var fileRepo = new Mock<IFileRepository>();

      fileRepo.Setup(fr => fr.FolderExists(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(true);

      byte[] buffer = {1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3};

      fileRepo.Setup(fr => fr.GetFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
        .ReturnsAsync(new MemoryStream(buffer));

      var coordinateSystemFileContent = await TccHelper.GetFileContentFromTcc(_businessCenterFile, _log, serviceExceptionHandler, fileRepo.Object);

      Assert.True(buffer.SequenceEqual(coordinateSystemFileContent), "CoordinateSystemFileContent not read from DC.");
    }

    [Fact]
    public async Task CreateProjectV5TBCExecutor_HappyPath()
    {
      var request = CreateProjectV5Request.CreateACreateProjectV5Request
      ("projectName", _boundaryLL, _businessCenterFile);
      var projectValidation = MapV5Models.MapCreateProjectV5RequestToProjectValidation(request, _customerUid.ToString());
      Assert.Equal(_checkBoundaryString, projectValidation.ProjectBoundaryWKT);
      var coordSystemFileContent = "Some dummy content";
      projectValidation.CoordinateSystemFileContent = System.Text.Encoding.ASCII.GetBytes(coordSystemFileContent);

      var createProjectResponseModel = new CreateProjectResponseModel() { TRN = _projectTrn };
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn, request.ProjectName);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(pr => pr.CreateProject(It.IsAny<CreateProjectRequestModel>(), _customHeaders)).ReturnsAsync(createProjectResponseModel);
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
      httpContextAccessor.HttpContext.Request.Path = new PathString("/api/v5/projects");

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
      
      var projectConfigurationModel = new ProjectConfigurationModel
      {
        FileName = "some coord sys file",
        FileDownloadLink = "some download link"
      };
      var cwsProfileSettingsClient = new Mock<ICwsProfileSettingsClient>();
      cwsProfileSettingsClient.Setup(ps => ps.GetProjectConfiguration(It.IsAny<Guid>(), ProjectConfigurationFileType.CALIBRATION, _customHeaders))
        .ReturnsAsync((ProjectConfigurationModel) null);
      cwsProfileSettingsClient.Setup(ps => ps.SaveProjectConfiguration(It.IsAny<Guid>(), ProjectConfigurationFileType.CALIBRATION, It.IsAny<ProjectConfigurationFileRequestModel>(), _customHeaders))
        .ReturnsAsync(projectConfigurationModel);

      var authn = new Mock<ITPaaSApplicationAuthentication>();
      authn.Setup(a => a.GetApplicationBearerToken()).Returns("some token");

      var executor = RequestExecutorContainerFactory.Build<CreateProjectTBCExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        productivity3dV1ProxyCoord.Object, httpContextAccessor: httpContextAccessor,
        dataOceanClient: dataOceanClient.Object, authn: authn.Object,
        cwsProjectClient: cwsProjectClient.Object, 
        cwsProfileSettingsClient: cwsProfileSettingsClient.Object);
      var result = await executor.ProcessAsync(projectValidation) as ProjectV6DescriptorsSingleResult;

      Assert.NotNull(result);
      Assert.False(string.IsNullOrEmpty(result.ProjectDescriptor.ProjectUid));
      Assert.True(result.ProjectDescriptor.ShortRaptorProjectId != 0);
      Assert.Equal(request.ProjectName, result.ProjectDescriptor.Name);
    }
  }
}
