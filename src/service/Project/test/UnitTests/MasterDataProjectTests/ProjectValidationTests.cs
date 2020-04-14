using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Exceptions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.ProjectTests
{
  public class ProjectValidationTestsDiFixture : UnitTestsDIFixture<ProjectValidationTestsDiFixture>
  {
    private static List<TBCPoint> _boundaryLL;
    private static BusinessCenterFile _businessCenterFile;
    private static string _checkBoundaryString;
    private readonly string _validBoundary =
      "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))";
    private readonly string _invalidBoundary = "blah";
    private static string _customerUid;


    public ProjectValidationTestsDiFixture()
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
    public void ValidateCreateProjectV5Request_HappyPath()
    {
      var request = CreateProjectV5Request.CreateACreateProjectV5Request
      (ProjectType.Standard, new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "projectName",
        "New Zealand Standard Time", _boundaryLL, _businessCenterFile);
      var createProjectEvent = MapV5Models.MapCreateProjectV5RequestToEvent(request, _customerUid);
      Assert.Equal(_checkBoundaryString, createProjectEvent.ProjectBoundary);

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.ProjectExists(It.IsAny<string>())).ReturnsAsync(false);

      ProjectDataValidator.Validate(createProjectEvent, projectRepo.Object, ServiceExceptionHandler);
      request.CoordinateSystem = ProjectDataValidator.ValidateBusinessCentreFile(request.CoordinateSystem);
    }

    [Fact]
    public void ValidateCreateProjectV5Request_BoundaryTooFewPoints()
    {
      var invalidBoundaryLl = new List<TBCPoint>
                              {
        new TBCPoint(-43.5, 172.6)
      };

      var request = CreateProjectV5Request.CreateACreateProjectV5Request
      (ProjectType.Standard, new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "projectName",
        "New Zealand Standard Time", invalidBoundaryLl, _businessCenterFile);
      var createProjectEvent = MapV5Models.MapCreateProjectV5RequestToEvent(request, _customerUid);

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.ProjectExists(It.IsAny<string>())).ReturnsAsync(false);

      var ex = Assert.Throws<ServiceException>(
        () => ProjectDataValidator.Validate(createProjectEvent, projectRepo.Object, ServiceExceptionHandler));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2024", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(-43.5, -200)]
    [InlineData(-43.5, 200)]
    [InlineData(-90.5, -100)]
    [InlineData(90.5, 100)]
    [InlineData(0.1, -1.99)]
    [InlineData(-1.99, 0.99)]
    public void ValidateCreateProjectV5Request_BoundaryInvalidLAtLong(double latitude, double longitude)
    {
      var invalidBoundaryLl = new List<TBCPoint>
                              {
        new TBCPoint(-43.5003, 172.6),
        new TBCPoint(latitude, longitude),
        new TBCPoint(-43.5003, 172.603),
        new TBCPoint(-43.5, 172.603)
      };

      var request = CreateProjectV5Request.CreateACreateProjectV5Request
      (ProjectType.Standard, new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "projectName",
        "New Zealand Standard Time", invalidBoundaryLl, _businessCenterFile);
      var createProjectEvent = MapV5Models.MapCreateProjectV5RequestToEvent(request, _customerUid);

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.ProjectExists(It.IsAny<string>())).ReturnsAsync(false);

      var ex = Assert.Throws<ServiceException>(
        () => ProjectDataValidator.Validate(createProjectEvent, projectRepo.Object, ServiceExceptionHandler));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2111", StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateCreateProjectV5Request_CheckBusinessCentreFile()
    {
      var bcf = BusinessCenterFile.CreateBusinessCenterFile(_businessCenterFile.FileSpaceId, _businessCenterFile.Path,
        _businessCenterFile.Name, _businessCenterFile.CreatedUtc);
      bcf.Path = "BC Data/Sites/Chch Test Site/";

      var resultantBusinessCenterFile = ProjectDataValidator.ValidateBusinessCentreFile(bcf);
      Assert.Equal("/BC Data/Sites/Chch Test Site", resultantBusinessCenterFile.Path);

      bcf = BusinessCenterFile.CreateBusinessCenterFile(_businessCenterFile.FileSpaceId, _businessCenterFile.Path,
        _businessCenterFile.Name, _businessCenterFile.CreatedUtc);
      bcf.Path = "";
      var ex = Assert.Throws<ServiceException>(() => ProjectDataValidator.ValidateBusinessCentreFile(bcf));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2083", StringComparison.Ordinal));

      bcf = BusinessCenterFile.CreateBusinessCenterFile(_businessCenterFile.FileSpaceId, _businessCenterFile.Path,
        _businessCenterFile.Name, _businessCenterFile.CreatedUtc);
      bcf.Name = "";
      ex = Assert.Throws<ServiceException>(() => ProjectDataValidator.ValidateBusinessCentreFile(bcf));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2002", StringComparison.Ordinal));

      bcf = BusinessCenterFile.CreateBusinessCenterFile(_businessCenterFile.FileSpaceId, _businessCenterFile.Path,
        _businessCenterFile.Name, _businessCenterFile.CreatedUtc);
      bcf.FileSpaceId = null;
      ex = Assert.Throws<ServiceException>(() => ProjectDataValidator.ValidateBusinessCentreFile(bcf));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2084", StringComparison.Ordinal));

      ex = Assert.Throws<ServiceException>(() => ProjectDataValidator.ValidateBusinessCentreFile(null));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2082", StringComparison.Ordinal));

    }

    [Fact]
    public void ValidateUpsertProjectV1Request_GoodBoundary()
    {
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, "the projectName", "the project description",
        new DateTime(2017, 01, 20), null, null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.ProjectExists(It.IsAny<string>())).ReturnsAsync(true);
      projectRepo.Setup(ps => ps.GetProjectOnly(It.IsAny<string>())).ReturnsAsync(new ProjectDatabaseModel
      {
        ProjectUID = updateProjectEvent.ProjectUID.ToString(),
        StartDate = updateProjectEvent.ProjectEndDate.AddDays(-2),
        ProjectTimeZone = updateProjectEvent.ProjectTimezone

      });

      ProjectDataValidator.Validate(updateProjectEvent, projectRepo.Object, ServiceExceptionHandler);
    }

    [Fact]
    public void ValidateUpsertProjectV1Request_InvalidBoundary()
    {
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, "the projectName", "the project description",
        new DateTime(2017, 01, 20), null, null, _invalidBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.ProjectExists(It.IsAny<string>())).ReturnsAsync(true);
      projectRepo.Setup(ps => ps.GetProjectOnly(It.IsAny<string>())).ReturnsAsync(new ProjectDatabaseModel
      {
        ProjectUID = updateProjectEvent.ProjectUID.ToString(),
        StartDate = updateProjectEvent.ProjectEndDate.AddDays(-2),
        ProjectTimeZone = updateProjectEvent.ProjectTimezone

      });

      var ex = Assert.Throws<ServiceException>(
        () => ProjectDataValidator.Validate(updateProjectEvent, projectRepo.Object, ServiceExceptionHandler));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2025", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ValidateUpsertProjectV4Request_DuplicateProjectName_NoneHappyPath()
    {
      string projectName = "the projectName";
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, projectName, "the project description",
        new DateTime(2017, 01, 20), null, null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectList = new List<ProjectDatabaseModel>();
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      await ProjectDataValidator.ValidateProjectName(_customerUid, projectName, request.ProjectUid.ToString(),
        Log, ServiceExceptionHandler, projectRepo.Object);
    }

    [Fact]
    public async Task ValidateUpsertProjectV4Request_DuplicateProjectName_SameProjectHappyPath()
    {
      var log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ProjectValidationTestsDiFixture>();
      string projectName = "the projectName";
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, projectName, "the project description",
        new DateTime(2017, 01, 20), null, null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectList = new List<Productivity3D.Project.Abstractions.Models.DatabaseModels.Project> { new ProjectDatabaseModel { Name = request.ProjectName, ProjectUID = request.ProjectUid.ToString() } };
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      await ProjectDataValidator.ValidateProjectName(_customerUid, projectName, request.ProjectUid.ToString(),
        log, ServiceExceptionHandler, projectRepo.Object);
    }

    [Fact]
    public async Task ValidateUpsertProjectV4Request_DuplicateProjectName_OtherProject()
    {
      string projectName = "the projectName";
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, projectName, "the project description",
        new DateTime(2017, 01, 20), null, null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectList = new List<ProjectDatabaseModel>
                        {
        new ProjectDatabaseModel { Name = request.ProjectName, ProjectUID = Guid.NewGuid().ToString() }
      };
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      var ex = await Assert.ThrowsAsync<ServiceException>(
        () => ProjectDataValidator.ValidateProjectName(_customerUid, projectName, request.ProjectUid.ToString(),
          Log, ServiceExceptionHandler, projectRepo.Object));

      Assert.NotEqual(-1, ex.GetContent.IndexOf("2109", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ValidateUpsertProjectV4Request_DuplicateProjectName_SameProjectAndOther()
    {
      string projectName = "the projectName";
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, projectName, "the project description",
        new DateTime(2017, 01, 20), null, null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectList = new List<ProjectDatabaseModel>
                        {
        new ProjectDatabaseModel {Name = request.ProjectName, ProjectUID = Guid.NewGuid().ToString()},
        new ProjectDatabaseModel { Name = request.ProjectName, ProjectUID = request.ProjectUid.ToString()}
      };
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      var ex = await Assert.ThrowsAsync<ServiceException>(
        () => ProjectDataValidator.ValidateProjectName(_customerUid, projectName, request.ProjectUid.ToString(),
          Log, ServiceExceptionHandler, projectRepo.Object));

      Assert.NotEqual(-1, ex.GetContent.IndexOf("2109", StringComparison.Ordinal));
      Assert.True(ex.GetContent.Contains("Not allowed duplicate, active projectnames: Count:1"), "should be 1 duplicate");
    }

    [Fact]
    public async Task ValidateUpsertProjectV4Request_DuplicateProjectName_MultiMatch()
    {
      // note that this should NEVER occur as the first duplicate shouldn't have been allowed
      string projectName = "the projectName";
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, projectName, "the project description",
        new DateTime(2017, 01, 20), null, null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectList = new List<ProjectDatabaseModel>
                        {
        new ProjectDatabaseModel {Name = request.ProjectName, ProjectUID = Guid.NewGuid().ToString()},
        new ProjectDatabaseModel {Name = request.ProjectName, ProjectUID = Guid.NewGuid().ToString()},
        new ProjectDatabaseModel { Name = request.ProjectName, ProjectUID = request.ProjectUid.ToString()}
      };
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      var ex = await Assert.ThrowsAsync<ServiceException>(
        () => ProjectDataValidator.ValidateProjectName(_customerUid, projectName, request.ProjectUid.ToString(),
          Log, ServiceExceptionHandler, projectRepo.Object));

      Assert.NotEqual(-1, ex.GetContent.IndexOf("2109", StringComparison.Ordinal));
      Assert.True(ex.GetContent.Contains("Not allowed duplicate, active projectnames: Count:2"), "should be 2 duplicate2");
    }
  }
}
