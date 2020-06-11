using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models.Cws;
using Xunit;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class ValidateProjectExecutorTests : UnitTestsDIFixture<CreateProjectExecutorTests>
  {
    public ValidateProjectExecutorTests()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_Valid()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var coordSystemResult = new CoordinateSystemSettingsResult();
      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(coordSystemResult);
      
      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = "some project",
        ProjectType = CwsProjectType.AcceptsTagFiles,
        Boundary = CreateNonOverlappingBoundary(),
        UpdateType = ProjectUpdateType.Created,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] {1,2,3,4,5,6,7,8}
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        coordProxy.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_MissingType()
    {
      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = "some project",
        ProjectType = null,
        Boundary = CreateNonOverlappingBoundary(),
        UpdateType = ProjectUpdateType.Created,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(130, result.Code);
      Assert.Equal("Missing project type.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_MissingCoordSysFileName()
    {
      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = "some project",
        ProjectType = CwsProjectType.AcceptsTagFiles,
        Boundary = CreateNonOverlappingBoundary(),
        UpdateType = ProjectUpdateType.Created,
        CoordinateSystemFileName = null,
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(132, result.Code);
      Assert.Equal("Missing coordinate system file name.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_MissingCoordSysFileContents()
    {
      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = "some project",
        ProjectType = CwsProjectType.AcceptsTagFiles,
        Boundary = CreateNonOverlappingBoundary(),
        UpdateType = ProjectUpdateType.Created,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = null
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(133, result.Code);
      Assert.Equal("Missing coordinate system file contents.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_MissingName()
    {
      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = null,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        Boundary = CreateNonOverlappingBoundary(),
        UpdateType = ProjectUpdateType.Created,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(11, result.Code);
      Assert.Equal("Missing Project Name.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_DuplicateName()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = projectList.Projects[0].ProjectName,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        Boundary = CreateNonOverlappingBoundary(),
        UpdateType = ProjectUpdateType.Created,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(109, result.Code);
      Assert.Equal("Project Name must be unique. 1 active project duplicates found.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_MissingBoundary()
    {
      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = "some project",
        ProjectType = CwsProjectType.AcceptsTagFiles,
        Boundary = null,
        UpdateType = ProjectUpdateType.Created,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(8, result.Code);
      Assert.Equal("Missing Project Boundary.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_InvalidBoundary()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = "some project",
        ProjectType = CwsProjectType.AcceptsTagFiles,
        Boundary = CreateInvalidBoundary(),
        UpdateType = ProjectUpdateType.Created,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(24, result.Code);
      Assert.Equal("Invalid project boundary as it should contain at least 3 points.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_SelfIntersectingBoundary()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = "some project",
        ProjectType = CwsProjectType.AcceptsTagFiles,
        Boundary = CreateSelfIntersectingBoundary(),
        UpdateType = ProjectUpdateType.Created,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(129, result.Code);
      Assert.Equal("Self-intersecting project boundary.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_OverlappingBoundary()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = "some project",
        ProjectType = CwsProjectType.AcceptsTagFiles,
        Boundary = projectList.Projects[0].ProjectSettings.Boundary,
        UpdateType = ProjectUpdateType.Created,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(43, result.Code);
      Assert.Equal("Project boundary overlaps another project.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_Update_MissingProject()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn, ProjectTrn = null, ProjectName = "some new project name", UpdateType = ProjectUpdateType.Updated
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(5, result.Code);
      Assert.Equal("Missing ProjectUID.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateName_Valid()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn, ProjectTrn = _projectTrn, ProjectName = "some new project name", UpdateType = ProjectUpdateType.Updated
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateName_Duplicate()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto { AccountTrn = _customerTrn, ProjectTrn = TRNHelper.MakeTRN(Guid.NewGuid()), ProjectName = projectList.Projects[0].ProjectName, UpdateType = ProjectUpdateType.Updated };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(109, result.Code);
      Assert.Equal("Project Name must be unique. 1 active project duplicates found.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateBoundary_Valid()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto {AccountTrn = _customerTrn, ProjectTrn = _projectTrn, Boundary = CreateNonOverlappingBoundary(), UpdateType = ProjectUpdateType.Updated};
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateBoundary_Overlapping()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto { AccountTrn = _customerTrn, ProjectTrn = TRNHelper.MakeTRN(Guid.NewGuid()), Boundary = projectList.Projects[0].ProjectSettings.Boundary, UpdateType = ProjectUpdateType.Updated };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(43, result.Code);
      Assert.Equal("Project boundary overlaps another project.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateProjectType_Valid()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);
      
      var coordSystemResult = new CoordinateSystemSettingsResult();
      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(coordSystemResult);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        UpdateType = ProjectUpdateType.Updated,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        coordProxy.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateProjectType_MissingCoordSysFileName()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        UpdateType = ProjectUpdateType.Updated,
        CoordinateSystemFileName = null,
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(132, result.Code);
      Assert.Equal("Missing coordinate system file name.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateProjectType_MissingCoordSysFileContents()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        UpdateType = ProjectUpdateType.Updated,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = null
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(133, result.Code);
      Assert.Equal("Missing coordinate system file contents.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateProjectType_MissingProject()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync((ProjectDetailResponseModel)null);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        UpdateType = ProjectUpdateType.Updated,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(7, result.Code);
      Assert.Equal("Project does not exist.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateCoordSysFile_Valid()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var coordSystemResult = new CoordinateSystemSettingsResult();
      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(coordSystemResult);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        UpdateType = ProjectUpdateType.Updated,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        coordProxy.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateCoordSysFile_MissingCoordSysFileName()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var coordSystemResult = new CoordinateSystemSettingsResult();
      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(coordSystemResult);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        UpdateType = ProjectUpdateType.Updated,
        CoordinateSystemFileName = null,
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        coordProxy.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(134, result.Code);
      Assert.Equal("Both coordinate system file name and contents must be provided.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateCoordSysFile_MissingCoordSysFileContents()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var coordSystemResult = new CoordinateSystemSettingsResult();
      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(coordSystemResult);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        UpdateType = ProjectUpdateType.Updated,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = null
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        coordProxy.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(134, result.Code);
      Assert.Equal("Both coordinate system file name and contents must be provided.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateCoordSysFile_WithException()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var exMessage = "some problem here";
      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ThrowsAsync(new Exception(exMessage));

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        UpdateType = ProjectUpdateType.Updated,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        coordProxy.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(57, result.Code);
      Assert.Equal($"A problem occurred at the validate CoordinateSystem endpoint in 3dpm. Exception: {exMessage}", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateCoordSysFile_NoResult()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync((CoordinateSystemSettingsResult)null);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        UpdateType = ProjectUpdateType.Updated,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        coordProxy.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(46, result.Code);
      Assert.Equal("Invalid CoordinateSystem.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateCoordSysFile_Failed()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var coordSystemResult = new CoordinateSystemSettingsResult{Code = 99, Message = "Failed!"};
      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(coordSystemResult);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        UpdateType = ProjectUpdateType.Updated,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        coordProxy.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(47, result.Code);
      Assert.Equal($"Unable to validate CoordinateSystem in 3dpm: {coordSystemResult.Code} {coordSystemResult.Message}.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_Delete_Valid()
    {
      var request = new ProjectValidateDto {AccountTrn = _customerTrn, ProjectTrn = _projectTrn, UpdateType = ProjectUpdateType.Deleted};
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: null);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_Delete_MissingProject()
    {
      var request = new ProjectValidateDto { AccountTrn = _customerTrn, ProjectTrn = null, UpdateType = ProjectUpdateType.Deleted };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: null);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(5, result.Code);
      Assert.Equal("Missing ProjectUID.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_MismatchedCustomerUid()
    {
      var request = new ProjectValidateDto { AccountTrn = TRNHelper.MakeTRN(Guid.NewGuid(), TRNHelper.TRN_ACCOUNT)};
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: null);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(135, result.Code);
      Assert.Equal("Mismatching customerUid.", result.Message);
    }

    private ProjectBoundary CreateNonOverlappingBoundary()
    {
      return new ProjectBoundary()
      {
        type = "Polygon",
        coordinates = new List<List<double[]>>
        {
          new List<double[]>
          {
            new[] {160.3, 1.7},
            new[] {160.4, 1.7},
            new[] {160.4, 1.8},
            new[] {160.4, 1.9},
            new[] {160.3, 1.7}
          }
        }
      };
    }

    private ProjectBoundary CreateInvalidBoundary()
    {
      return new ProjectBoundary()
      {
        type = "Polygon",
        coordinates = new List<List<double[]>>
        {
          new List<double[]>
          {
            new[] {160.3, 1.7},
            new[] {160.4, 1.7}
          }
        }
      };
    }

    private ProjectBoundary CreateSelfIntersectingBoundary()
    {
      return new ProjectBoundary()
      {
        type = "Polygon",
        coordinates = new List<List<double[]>>
        {
          new List<double[]>
          {
            new[] {160.3, 1.7},
            new[] {160.4, 1.9},
            new[] {160.3, 1.9},
            new[] {160.4, 1.7},
            new[] {160.3, 1.7}
          }
        }
      };
    }
  }
}
