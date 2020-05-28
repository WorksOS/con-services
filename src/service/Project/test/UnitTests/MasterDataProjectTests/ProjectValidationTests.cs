using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Exceptions;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.MasterData.ProjectTests
{
  public class ProjectValidationTests : UnitTestsDIFixture<ProjectValidationTests>
  {
    private readonly string _validBoundary =
      "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))";
    private readonly string _invalidBoundary = "blah";

    public ProjectValidationTests()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    [Fact]
    public void ValidateUpsertProjectRequest_GoodBoundary()
    {
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (_projectUid, ProjectType.Standard, "the projectName", "the project description", null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      ProjectDataValidator.Validate(updateProjectEvent, _customerUid, _userUid, _log, ServiceExceptionHandler, cwsProjectClient.Object, _customHeaders);
    }

    [Fact]
    public void ValidateUpsertProjectRequest_InvalidBoundary()
    {
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (_projectUid, ProjectType.Standard, "the projectName", null, null, _invalidBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var ex = Assert.Throws<ServiceException>(
        () => ProjectDataValidator.Validate(updateProjectEvent, _customerUid, _userUid, _log, ServiceExceptionHandler, cwsProjectClient.Object, _customHeaders));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2025", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ValidateUpsertProjectV4Request_DuplicateProjectName_NoneHappyPath()
    {
      var projectName = "the projectName";
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (_projectUid, ProjectType.Standard, projectName, "the project description", null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectList = CreateProjectListModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      await ProjectDataValidator.ValidateProjectName(_customerUid, _userUid, projectName, request.ProjectUid,
        _log, ServiceExceptionHandler, cwsProjectClient.Object, _customHeaders);
    }

    [Fact]
    public async Task ValidateUpsertProjectV4Request_DuplicateProjectName_SameProjectHappyPath()
    {
      var log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ProjectValidationTests>();
      string projectName = "the projectName";
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (_projectUid, ProjectType.Standard, projectName, null, null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectList = new ProjectDetailListResponseModel()
      {
        Projects = new List<ProjectDetailResponseModel>()
        {
          CreateProjectDetailModel(_customerTrn, _projectTrn, request.ProjectName)
        }
      };
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      await ProjectDataValidator.ValidateProjectName(_customerUid, _userUid, projectName, request.ProjectUid,
        _log, ServiceExceptionHandler, cwsProjectClient.Object, _customHeaders);
    }

    [Fact]
    public async Task ValidateUpsertProjectV4Request_DuplicateProjectName_OtherProject()
    {
      var projectName = "the projectName";
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (_projectUid, ProjectType.Standard, projectName, null, null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectList = new ProjectDetailListResponseModel()
      {
        Projects = new List<ProjectDetailResponseModel>()
        {
          CreateProjectDetailModel(_customerTrn, TRNHelper.MakeTRN(Guid.NewGuid()), request.ProjectName)
        }
      };
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var ex = await Assert.ThrowsAsync<ServiceException>(
        () => ProjectDataValidator.ValidateProjectName(_customerUid, _userUid, projectName, request.ProjectUid,
          _log, ServiceExceptionHandler, cwsProjectClient.Object, _customHeaders));

      Assert.Equal(2109, ex.GetResult.Code);
    }

    [Fact]
    public async Task ValidateUpsertProjectV4Request_DuplicateProjectName_SameProjectAndOther()
    {
      var projectName = "the projectName";
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (_projectUid, ProjectType.Standard, projectName, null, null, _validBoundary);

      var projectList = new ProjectDetailListResponseModel()
      {
        Projects = new List<ProjectDetailResponseModel>()
        {
          CreateProjectDetailModel(_customerTrn, TRNHelper.MakeTRN(Guid.NewGuid()), request.ProjectName),
          CreateProjectDetailModel(_customerTrn, _projectTrn, request.ProjectName)
        }
      };
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var ex = await Assert.ThrowsAsync<ServiceException>(
        () => ProjectDataValidator.ValidateProjectName(_customerUid, _userUid, projectName, request.ProjectUid,
          _log, ServiceExceptionHandler, cwsProjectClient.Object, _customHeaders));

      Assert.Equal(2109, ex.GetResult.Code);
    }

    [Fact]
    public async Task ValidateUpsertProjectV4Request_DuplicateProjectName_MultiMatch()
    {
      // note that this should NEVER occur as the first duplicate shouldn't have been allowed
      var projectName = "the projectName";
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (_projectUid, ProjectType.Standard, projectName, null, null, _validBoundary);

      var projectList = new ProjectDetailListResponseModel()
      {
        Projects = new List<ProjectDetailResponseModel>()
        {
          CreateProjectDetailModel(_customerTrn, TRNHelper.MakeTRN(Guid.NewGuid()), request.ProjectName),
          CreateProjectDetailModel(_customerTrn, TRNHelper.MakeTRN(Guid.NewGuid()), request.ProjectName),
          CreateProjectDetailModel(_customerTrn, _projectTrn, request.ProjectName)
        }
      };
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(projectList);

      var ex = await Assert.ThrowsAsync<ServiceException>(
        () => ProjectDataValidator.ValidateProjectName(_customerUid, _userUid, projectName, request.ProjectUid,
          _log, ServiceExceptionHandler, cwsProjectClient.Object, _customHeaders));

      Assert.Equal(2109, ex.GetResult.Code);
    }
  }
}
