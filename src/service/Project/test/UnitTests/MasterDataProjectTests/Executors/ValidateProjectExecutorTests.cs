using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
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

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = "some project",
        ProjectType = CwsProjectType.ThreeDEnabled,
        Boundary = CreateNonOverlappingBoundary(),
        UpdateType = ProjectUpdateType.Created
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
    public async Task ValidateProjectExecutor_Create_MissingName()
    {
      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = null,
        ProjectType = CwsProjectType.ThreeDEnabled,
        Boundary = CreateNonOverlappingBoundary(),
        UpdateType = ProjectUpdateType.Created
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(11, result.Code);
      Assert.Equal("Missing ProjectName.", result.Message);
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
        UpdateType = ProjectUpdateType.Created
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
        ProjectType = CwsProjectType.ThreeDEnabled,
        Boundary = CreateNonOverlappingBoundary(),
        UpdateType = ProjectUpdateType.Created
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(109, result.Code);
      Assert.Equal("ProjectName must be unique. 1 active project duplicates found.", result.Message);
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_MissingBoundary()
    {
      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = "some project",
        ProjectType = CwsProjectType.ThreeDEnabled,
        Boundary = null,
        UpdateType = ProjectUpdateType.Created
      };
      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(request);
      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders);
      var result = await executor.ProcessAsync(data);
      Assert.Equal(8, result.Code);
      Assert.Equal("Missing ProjectBoundary.", result.Message);
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
        ProjectType = CwsProjectType.ThreeDEnabled,
        Boundary = CreateInvalidBoundary(),
        UpdateType = ProjectUpdateType.Created
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
        ProjectType = CwsProjectType.ThreeDEnabled,
        Boundary = CreateSelfIntersectingBoundary(),
        UpdateType = ProjectUpdateType.Created
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
        ProjectType = CwsProjectType.ThreeDEnabled,
        Boundary = projectList.Projects[0].ProjectSettings.Boundary,
        UpdateType = ProjectUpdateType.Created
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

      var request = new ProjectValidateDto {AccountTrn = _customerTrn, ProjectTrn = _projectTrn, ProjectName = "some new project name", UpdateType = ProjectUpdateType.Updated};
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
      Assert.Equal("ProjectName must be unique. 1 active project duplicates found.", result.Message);
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

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        ProjectType = CwsProjectType.ThreeDEnabled,
        UpdateType = ProjectUpdateType.Updated
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
        ProjectType = CwsProjectType.ThreeDEnabled,
        UpdateType = ProjectUpdateType.Updated
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
