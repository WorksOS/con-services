using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.ProjectTests.Extensions;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models.Cws;
using Xunit;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class ValidateProjectExecutorTests : UnitTestsDIFixture<ValidateProjectExecutorTests>
  {
    private ProjectValidation MapProjectValidation(ProjectValidateDto request) => AutoMapperUtility.Automapper.Map<ProjectValidation>(request);

    private ValidateProjectExecutor CreateExecutor(
      IProductivity3dV1ProxyCoord productivity3dV1ProxyCoord = null,
      ICwsProjectClient cwsProjectClient = null,
      IProductivity3dV2ProxyCompaction productivity3dV2ProxyCompaction = null) =>
      RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        productivity3dV1ProxyCoord, cwsProjectClient: cwsProjectClient,
        productivity3dV2ProxyCompaction: productivity3dV2ProxyCompaction);

    public ValidateProjectExecutorTests()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_Valid()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

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
        UpdateType = CwsUpdateType.CreateProject,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var result = await CreateExecutor(coordProxy.Object, cwsProjectClient.Object)
        .ProcessAsync(MapProjectValidation(request));

      result.IsSuccessResponse();
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
        UpdateType = CwsUpdateType.CreateProject,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var result = await CreateExecutor().ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(130, "Missing project type.");
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
        UpdateType = CwsUpdateType.CreateProject,
        CoordinateSystemFileName = null,
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var result = await CreateExecutor().ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(132, "Missing coordinate system file name.");
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
        UpdateType = CwsUpdateType.CreateProject,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = null
      };

      var result = await CreateExecutor().ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(133, "Missing coordinate system file contents.");
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
        UpdateType = CwsUpdateType.CreateProject,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var result = await CreateExecutor().ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(11, "Missing Project Name.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_DuplicateName()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = projectList.Projects[0].ProjectName,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        Boundary = CreateNonOverlappingBoundary(),
        UpdateType = CwsUpdateType.CreateProject,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var executor = CreateExecutor(cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(109, "Project Name must be unique. 1 active project duplicates found.");
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
        UpdateType = CwsUpdateType.CreateProject,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var result = await CreateExecutor().ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(8, "Missing Project Boundary.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_InvalidBoundary()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = "some project",
        ProjectType = CwsProjectType.AcceptsTagFiles,
        Boundary = CreateInvalidBoundary(),
        UpdateType = CwsUpdateType.CreateProject,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var executor = CreateExecutor(cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(24, "Invalid project boundary as it should contain at least 3 points.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_SelfIntersectingBoundary()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = "some project",
        ProjectType = CwsProjectType.AcceptsTagFiles,
        Boundary = CreateSelfIntersectingBoundary(),
        UpdateType = CwsUpdateType.CreateProject,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var executor = CreateExecutor(cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(129, "Self-intersecting project boundary.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_Create_OverlappingBoundary()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = "some project",
        ProjectType = CwsProjectType.AcceptsTagFiles,
        Boundary = projectList.Projects[0].ProjectSettings.Boundary,
        UpdateType = CwsUpdateType.CreateProject,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var executor = CreateExecutor(cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(43, "Project boundary overlaps another project.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_Update_MissingProject()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = null,
        ProjectName = "some new project name",
        UpdateType = CwsUpdateType.UpdateProject
      };

      var executor = CreateExecutor(cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));
      result.ShouldBe(5, "Missing ProjectUID.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateName_Valid()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        ProjectName = "some new project name",
        UpdateType = CwsUpdateType.UpdateProject
      };

      var executor = CreateExecutor(cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.IsSuccessResponse();
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateName_Duplicate()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto { AccountTrn = _customerTrn, ProjectTrn = TRNHelper.MakeTRN(Guid.NewGuid()), ProjectName = projectList.Projects[0].ProjectName, UpdateType = CwsUpdateType.UpdateProject };

      var executor = RequestExecutorContainerFactory.Build<ValidateProjectExecutor>
      (_loggerFactory, _configStore, ServiceExceptionHandler,
        _customerUid.ToString(), _userUid.ToString(), null, _customHeaders,
        null, cwsProjectClient: cwsProjectClient.Object);

      var result = await executor.ProcessAsync(MapProjectValidation(request));
      result.ShouldBe(109, "Project Name must be unique. 1 active project duplicates found.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateBoundary_Valid()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto { AccountTrn = _customerTrn, ProjectTrn = _projectTrn, Boundary = CreateNonOverlappingBoundary(), UpdateType = CwsUpdateType.BoundaryUpdate };
      var executor = CreateExecutor(cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.IsSuccessResponse();
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateBoundary_Overlapping()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var request = new ProjectValidateDto { AccountTrn = _customerTrn, ProjectTrn = TRNHelper.MakeTRN(Guid.NewGuid()), Boundary = projectList.Projects[0].ProjectSettings.Boundary, UpdateType = CwsUpdateType.BoundaryUpdate };
      var executor = CreateExecutor(cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(43, "Project boundary overlaps another project.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateProjectType_Valid()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var coordSystemResult = new CoordinateSystemSettingsResult();
      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(coordSystemResult);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        UpdateType = CwsUpdateType.UpdateProjectType,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var executor = CreateExecutor(productivity3dV1ProxyCoord: coordProxy.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.IsSuccessResponse();
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateProjectType_MissingCoordSysFileName()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        UpdateType = CwsUpdateType.CalibrationUpdate,
        CoordinateSystemFileName = null,
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var executor = CreateExecutor(cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(132, "Missing coordinate system file name.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateProjectType_MissingCoordSysFileContents()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        UpdateType = CwsUpdateType.CalibrationUpdate,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = null
      };

      var executor = CreateExecutor(cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(133, "Missing coordinate system file contents.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateProjectType_MissingProject()
    {
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync((ProjectDetailResponseModel)null);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        UpdateType = CwsUpdateType.UpdateProjectType,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var executor = CreateExecutor(cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(7, "Project does not exist.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateCoordSysFile_Valid()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var coordSystemResult = new CoordinateSystemSettingsResult();
      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(coordSystemResult);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        UpdateType = CwsUpdateType.CalibrationUpdate,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var executor = CreateExecutor(productivity3dV1ProxyCoord: coordProxy.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.IsSuccessResponse();
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateCoordSysFile_MissingCoordSysFileName()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var coordSystemResult = new CoordinateSystemSettingsResult();
      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(coordSystemResult);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        UpdateType = CwsUpdateType.CalibrationUpdate,
        CoordinateSystemFileName = null,
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var executor = CreateExecutor(productivity3dV1ProxyCoord: coordProxy.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(134, "Both coordinate system file name and contents must be provided.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateCoordSysFile_MissingCoordSysFileContents()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var coordSystemResult = new CoordinateSystemSettingsResult();
      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(coordSystemResult);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        UpdateType = CwsUpdateType.CalibrationUpdate,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = null
      };

      var executor = CreateExecutor(productivity3dV1ProxyCoord: coordProxy.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(134, "Both coordinate system file name and contents must be provided.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateCoordSysFile_WithException()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      const string EX_MESSAGE = "some problem here";
      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ThrowsAsync(new Exception(EX_MESSAGE));

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        UpdateType = CwsUpdateType.CalibrationUpdate,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var executor = CreateExecutor(productivity3dV1ProxyCoord: coordProxy.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(57, $"A problem occurred at the validate CoordinateSystem endpoint in 3dpm. Exception: {EX_MESSAGE}");
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateCoordSysFile_NoResult()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync((CoordinateSystemSettingsResult)null);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        UpdateType = CwsUpdateType.CalibrationUpdate,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var executor = CreateExecutor(productivity3dV1ProxyCoord: coordProxy.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(46, "Invalid CoordinateSystem.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_UpdateCoordSysFile_Failed()
    {
      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CwsProjectType?>(), It.IsAny<ProjectStatus?>(), It.IsAny<bool>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var coordSystemResult = new CoordinateSystemSettingsResult { Code = 99, Message = "Failed!" };
      var coordProxy = new Mock<IProductivity3dV1ProxyCoord>();
      coordProxy.Setup(cp => cp.CoordinateSystemValidate(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(coordSystemResult);

      var request = new ProjectValidateDto
      {
        AccountTrn = _customerTrn,
        ProjectTrn = _projectTrn,
        UpdateType = CwsUpdateType.CalibrationUpdate,
        CoordinateSystemFileName = "some file name",
        CoordinateSystemFileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
      };

      var executor = CreateExecutor(productivity3dV1ProxyCoord: coordProxy.Object, cwsProjectClient: cwsProjectClient.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(47, $"Unable to validate CoordinateSystem in 3dpm: {coordSystemResult.Code} {coordSystemResult.Message}.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_Delete_Valid()
    {
      var extents = new ProjectStatisticsResult
      {
        extents = new BoundingBox3DGrid(BoundingBox3DGrid.MAX_RANGE, BoundingBox3DGrid.MAX_RANGE, BoundingBox3DGrid.MAX_RANGE, BoundingBox3DGrid.MIN_RANGE, BoundingBox3DGrid.MIN_RANGE, BoundingBox3DGrid.MIN_RANGE)
      };
      var extentsProxy = new Mock<IProductivity3dV2ProxyCompaction>();
      extentsProxy.Setup(ep => ep.GetProjectStatistics(It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(extents);

      var request = new ProjectValidateDto { AccountTrn = _customerTrn, ProjectTrn = _projectTrn, UpdateType = CwsUpdateType.DeleteProject };
      var executor = CreateExecutor(productivity3dV2ProxyCompaction: extentsProxy.Object);

      var result = await executor.ProcessAsync(MapProjectValidation(request));
      result.IsSuccessResponse();
    }

    [Fact]
    public async Task ValidateProjectExecutor_Delete_AssumedValid()
    {
      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError));

      var extentsProxy = new Mock<IProductivity3dV2ProxyCompaction>();
      extentsProxy.Setup(ep => ep.GetProjectStatistics(It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ThrowsAsync(exception);

      var request = new ProjectValidateDto { AccountTrn = _customerTrn, ProjectTrn = _projectTrn, UpdateType = CwsUpdateType.DeleteProject };
      var executor = CreateExecutor(productivity3dV2ProxyCompaction: extentsProxy.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));
      result.IsSuccessResponse();
    }

    [Fact]
    public async Task ValidateProjectExecutor_Delete_NoExtents()
    {
      var extents = new ProjectStatisticsResult();
      var extentsProxy = new Mock<IProductivity3dV2ProxyCompaction>();
      extentsProxy.Setup(ep => ep.GetProjectStatistics(It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(extents);

      var request = new ProjectValidateDto { AccountTrn = _customerTrn, ProjectTrn = _projectTrn, UpdateType = CwsUpdateType.DeleteProject };
      var executor = CreateExecutor(productivity3dV2ProxyCompaction: extentsProxy.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.IsSuccessResponse();
    }

    [Fact]
    public async Task ValidateProjectExecutor_Delete_HasTagFileData()
    {
      var extents = new ProjectStatisticsResult { extents = new BoundingBox3DGrid(10, 10, 10, 20, 20, 20) };
      var extentsProxy = new Mock<IProductivity3dV2ProxyCompaction>();
      extentsProxy.Setup(ep => ep.GetProjectStatistics(It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(extents);

      var request = new ProjectValidateDto { AccountTrn = _customerTrn, ProjectTrn = _projectTrn, UpdateType = CwsUpdateType.DeleteProject };
      var executor = CreateExecutor(productivity3dV2ProxyCompaction: extentsProxy.Object);
      var result = await executor.ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(141, "Cannot delete a project that has 3D production (tag file) data");
    }

    [Fact]
    public async Task ValidateProjectExecutor_Delete_MissingProject()
    {
      var request = new ProjectValidateDto { AccountTrn = _customerTrn, ProjectTrn = null, UpdateType = CwsUpdateType.DeleteProject };
      var result = await CreateExecutor().ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(5, "Missing ProjectUID.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_MismatchedCustomerUid()
    {
      var request = new ProjectValidateDto { AccountTrn = _customerTrn, UpdateType = CwsUpdateType.DeleteDeviceFromAccount };
      var result = await CreateExecutor().ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(136, "Unknown update type in project validation.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_UnknownUpdateType()
    {
      var request = new ProjectValidateDto { AccountTrn = TRNHelper.MakeTRN(Guid.NewGuid(), TRNHelper.TRN_ACCOUNT) };
      var result = await CreateExecutor().ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(135, "Mismatched customerUid.");
    }

    [Fact]
    public async Task ValidateProjectExecutor_Archive_Valid()
    {
      var request = new ProjectValidateDto { AccountTrn = _customerTrn, ProjectTrn = _projectTrn, UpdateType = CwsUpdateType.ArchiveProject };
      var result = await CreateExecutor().ProcessAsync(MapProjectValidation(request));

      result.IsSuccessResponse();
    }

    [Fact]
    public async Task ValidateProjectExecutor_Archive_MissingProject()
    {
      var request = new ProjectValidateDto { AccountTrn = _customerTrn, ProjectTrn = null, UpdateType = CwsUpdateType.ArchiveProject };
      var result = await CreateExecutor().ProcessAsync(MapProjectValidation(request));

      result.ShouldBe(5, "Missing ProjectUID.");
    }

    private ProjectBoundary CreateNonOverlappingBoundary() => new ProjectBoundary()
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

    private ProjectBoundary CreateInvalidBoundary() => new ProjectBoundary()
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

    private ProjectBoundary CreateSelfIntersectingBoundary() => new ProjectBoundary()
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
