using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ProjectValidationTests : ExecutorBaseTests
  {
    private static List<TBCPoint> _boundaryLL;
    private static BusinessCenterFile _businessCenterFile;
    private static string _checkBoundaryString;
    private readonly string _validBoundary;
    private readonly string _invalidBoundary;
    private static string _customerUid;


    public ProjectValidationTests()
    {
      _validBoundary =
        "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))";
      _invalidBoundary = "blah";
    }

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
      _boundaryLL = new List<TBCPoint>()
      {
        new TBCPoint(-43.5, 172.6),
        new TBCPoint(-43.5003, 172.6),
        new TBCPoint(-43.5003, 172.603),
        new TBCPoint(-43.5, 172.603)
      };

      _checkBoundaryString = "POLYGON((172.6 -43.5,172.6 -43.5003,172.603 -43.5003,172.603 -43.5,172.6 -43.5))";

      _businessCenterFile = new BusinessCenterFile()
      {
        FileSpaceId = "u3bdc38d-1afe-470e-8c1c-fc241d4c5e01",
        Path = "/BC Data/Sites/Chch Test Site",
        Name = "CTCTSITECAL.dc",
        CreatedUtc = DateTime.UtcNow.AddDays(-0.5)
      };

      _customerUid = Guid.NewGuid().ToString();
    }

    [TestMethod]
    public void ValidateCreateProjectV2Request_HappyPath()
    {
      var request = CreateProjectV2Request.CreateACreateProjectV2Request
      (ProjectType.Standard, new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "projectName",
        "New Zealand Standard Time", _boundaryLL, _businessCenterFile);
      var createProjectEvent = MapV2Models.MapCreateProjectV2RequestToEvent(request, _customerUid);
      Assert.AreEqual(_checkBoundaryString, createProjectEvent.ProjectBoundary, "Invalid ProjectBoundary in WKT");

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.ProjectExists(It.IsAny<string>())).ReturnsAsync(false);

      ProjectDataValidator.Validate(createProjectEvent, projectRepo.Object, ServiceExceptionHandler);
      request.CoordinateSystem = ProjectDataValidator.ValidateBusinessCentreFile(request.CoordinateSystem);
    }

    [TestMethod]
    public void ValidateCreateProjectV2Request_BoundaryTooFewPoints()
    {
      var invalidBoundaryLl = new List<TBCPoint>()
      {
        new TBCPoint(-43.5, 172.6)
      };

      var request = CreateProjectV2Request.CreateACreateProjectV2Request
      (ProjectType.Standard, new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "projectName",
        "New Zealand Standard Time", invalidBoundaryLl, _businessCenterFile);
      var createProjectEvent = MapV2Models.MapCreateProjectV2RequestToEvent(request, _customerUid);

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.ProjectExists(It.IsAny<string>())).ReturnsAsync(false);

      var ex = Assert.ThrowsException<ServiceException>(
        () => ProjectDataValidator.Validate(createProjectEvent, projectRepo.Object, ServiceExceptionHandler));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2024", StringComparison.Ordinal), "Expected error number 2024");
    }

    [TestMethod]
    public void ValidateCreateProjectV2Request_CheckBusinessCentreFile()
    {
      var bcf = BusinessCenterFile.CreateBusinessCenterFile(_businessCenterFile.FileSpaceId, _businessCenterFile.Path,
        _businessCenterFile.Name, _businessCenterFile.CreatedUtc);
      bcf.Path = "BC Data/Sites/Chch Test Site/";

      var resultantBusinessCenterFile = ProjectDataValidator.ValidateBusinessCentreFile(bcf);
      Assert.AreEqual("/BC Data/Sites/Chch Test Site", resultantBusinessCenterFile.Path,
        "Path should have bounding slashes inserted");

      bcf = BusinessCenterFile.CreateBusinessCenterFile(_businessCenterFile.FileSpaceId, _businessCenterFile.Path,
        _businessCenterFile.Name, _businessCenterFile.CreatedUtc);
      bcf.Path = "";
      var ex = Assert.ThrowsException<ServiceException>(() => ProjectDataValidator.ValidateBusinessCentreFile(bcf));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2083", StringComparison.Ordinal));

      bcf = BusinessCenterFile.CreateBusinessCenterFile(_businessCenterFile.FileSpaceId, _businessCenterFile.Path,
        _businessCenterFile.Name, _businessCenterFile.CreatedUtc);
      bcf.Name = "";
      ex = Assert.ThrowsException<ServiceException>(() => ProjectDataValidator.ValidateBusinessCentreFile(bcf));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2002", StringComparison.Ordinal));

      bcf = BusinessCenterFile.CreateBusinessCenterFile(_businessCenterFile.FileSpaceId, _businessCenterFile.Path,
        _businessCenterFile.Name, _businessCenterFile.CreatedUtc);
      bcf.FileSpaceId = null;
      ex = Assert.ThrowsException<ServiceException>(() => ProjectDataValidator.ValidateBusinessCentreFile(bcf));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2084", StringComparison.Ordinal));

      ex = Assert.ThrowsException<ServiceException>(() => ProjectDataValidator.ValidateBusinessCentreFile(null));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2082", StringComparison.Ordinal));

    }

    [TestMethod]
    public void ValidateUpsertProjectV1Request_GoodBoundary()
    {
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, "the projectName", "the project description",
        new DateTime(2017, 01, 20), null, null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.ProjectExists(It.IsAny<string>())).ReturnsAsync(true);
      projectRepo.Setup(ps => ps.GetProjectOnly(It.IsAny<string>())).ReturnsAsync(new Repositories.DBModels.Project()
      {
        ProjectUID = updateProjectEvent.ProjectUID.ToString(),
        StartDate = updateProjectEvent.ProjectEndDate.AddDays(-2),
        ProjectTimeZone = updateProjectEvent.ProjectTimezone

      });

      ProjectDataValidator.Validate(updateProjectEvent, projectRepo.Object, ServiceExceptionHandler);
    }

    [TestMethod]
    public void ValidateUpsertProjectV1Request_InvalidBoundary()
    {
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, "the projectName", "the project description",
        new DateTime(2017, 01, 20), null, null, _invalidBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.ProjectExists(It.IsAny<string>())).ReturnsAsync(true);
      projectRepo.Setup(ps => ps.GetProjectOnly(It.IsAny<string>())).ReturnsAsync(new Repositories.DBModels.Project()
      {
        ProjectUID = updateProjectEvent.ProjectUID.ToString(),
        StartDate = updateProjectEvent.ProjectEndDate.AddDays(-2),
        ProjectTimeZone = updateProjectEvent.ProjectTimezone

      });

      var ex = Assert.ThrowsException<ServiceException>(
        () => ProjectDataValidator.Validate(updateProjectEvent, projectRepo.Object, ServiceExceptionHandler));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2025", StringComparison.Ordinal), "Expected error number 2025");
    }

    
    [TestMethod]
    public void ValidateGeofenceTypes_HappyPath()
    {
      var geofenceTypes= new List<GeofenceType>() { GeofenceType.Landfill };
      ProjectDataValidator.ValidateGeofenceTypes(geofenceTypes);
    }

    [TestMethod]
    public void ValidateGeofenceTypes_LandfillTypeRequired1()
    {
      var geofenceTypes = new List<GeofenceType>() { GeofenceType.Generic, GeofenceType.Waste };
      var ex = Assert.ThrowsException<ServiceException>(
        () => ProjectDataValidator.ValidateGeofenceTypes(geofenceTypes));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2102", StringComparison.Ordinal), "Expected error number 2102");
    }

    [TestMethod]
    public void ValidateGeofenceTypes_LandfillTypeRequired2()
    {
      var geofenceTypes = new List<GeofenceType>() { GeofenceType.Filter, GeofenceType.Landfill, GeofenceType.Borrow };
      var ex = Assert.ThrowsException<ServiceException>(
        () => ProjectDataValidator.ValidateGeofenceTypes(geofenceTypes));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2102", StringComparison.Ordinal), "Expected error number 2102");
    }


    [TestMethod]
    public void ValidateGeofenceTypes_UnhappyPath1()
    {
      var ex = Assert.ThrowsException<ServiceException>(
        () => ProjectDataValidator.ValidateGeofenceTypes(null));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2073", StringComparison.Ordinal), "Expected error number 2073");
    }

    [TestMethod]
    public void ValidateGeofenceTypes_UnhappyPath2()
    {
      var geofenceTypes = new List<GeofenceType>() {GeofenceType.Waste};
      var ex = Assert.ThrowsException<ServiceException>(
        () => ProjectDataValidator.ValidateGeofenceTypes(geofenceTypes));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2102", StringComparison.Ordinal), "Expected error number 2102");
    }

    [TestMethod]
    public void ValidateGeofenceTypes_UnhappyPath3()
    {
      var geofenceTypes = new List<GeofenceType>();
      var ex = Assert.ThrowsException<ServiceException>(
        () => ProjectDataValidator.ValidateGeofenceTypes(geofenceTypes));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2073", StringComparison.Ordinal), "Expected error number 2073");
    }

    [TestMethod]
    public async Task ValidateUpsertProjectV4Request_DuplicateProjectName_NoneHappyPath()
    {
      var log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ProjectValidationTests>();
      string projectName = "the projectName";
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, projectName, "the project description",
        new DateTime(2017, 01, 20), null, null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectList = new List<Repositories.DBModels.Project>();
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      await ProjectDataValidator.ValidateProjectName(_customerUid, projectName, request.ProjectUid.ToString(),
        log, ServiceExceptionHandler, projectRepo.Object);
    }

    [TestMethod]
    public async Task ValidateUpsertProjectV4Request_DuplicateProjectName_SameProjectHappyPath()
    {
      var log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ProjectValidationTests>();
      string projectName = "the projectName";
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, projectName, "the project description",
        new DateTime(2017, 01, 20), null, null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectList = new List<Repositories.DBModels.Project>(){ new Repositories.DBModels.Project() {Name = request.ProjectName, ProjectUID = request.ProjectUid.ToString()} };
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      await ProjectDataValidator.ValidateProjectName(_customerUid, projectName, request.ProjectUid.ToString(),
        log, ServiceExceptionHandler, projectRepo.Object);
    }

    [TestMethod]
    public async Task ValidateUpsertProjectV4Request_DuplicateProjectName_OtherProject()
    {
      var log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ProjectValidationTests>();
      string projectName = "the projectName";
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, projectName, "the project description",
        new DateTime(2017, 01, 20), null, null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectList = new List<Repositories.DBModels.Project>()
      {
        new Repositories.DBModels.Project() { Name = request.ProjectName, ProjectUID = Guid.NewGuid().ToString() }
      };
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(
        () => ProjectDataValidator.ValidateProjectName(_customerUid, projectName, request.ProjectUid.ToString(),
          log, ServiceExceptionHandler, projectRepo.Object));

      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2109", StringComparison.Ordinal), "Expected error number 2109");
    }

    [TestMethod]
    public async Task ValidateUpsertProjectV4Request_DuplicateProjectName_SameProjectAndOther()
    {
      var log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ProjectValidationTests>();
      string projectName = "the projectName";
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, projectName, "the project description",
        new DateTime(2017, 01, 20), null, null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectList = new List<Repositories.DBModels.Project>()
      {
        new Repositories.DBModels.Project(){Name = request.ProjectName, ProjectUID = Guid.NewGuid().ToString()},
        new Repositories.DBModels.Project() { Name = request.ProjectName, ProjectUID = request.ProjectUid.ToString()}
      };
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(
        () => ProjectDataValidator.ValidateProjectName(_customerUid, projectName, request.ProjectUid.ToString(),
          log, ServiceExceptionHandler, projectRepo.Object));

      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2109", StringComparison.Ordinal), "Expected error number 2109");
      Assert.IsTrue(ex.GetContent.Contains("Not allowed duplicate, active projectnames: Count:1"), "should be 1 duplicate");
    }

    [TestMethod]
    public async Task ValidateUpsertProjectV4Request_DuplicateProjectName_MultiMatch()
    {
      // note that this should NEVER occur as the first duplicate shouldn't have been allowed
      var log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ProjectValidationTests>();
      string projectName = "the projectName";
      var request = UpdateProjectRequest.CreateUpdateProjectRequest
      (Guid.NewGuid(), ProjectType.Standard, projectName, "the project description",
        new DateTime(2017, 01, 20), null, null, _validBoundary);

      var updateProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(request);
      updateProjectEvent.ActionUTC = DateTime.UtcNow;

      var projectList = new List<Repositories.DBModels.Project>()
      {
        new Repositories.DBModels.Project(){Name = request.ProjectName, ProjectUID = Guid.NewGuid().ToString()},
        new Repositories.DBModels.Project(){Name = request.ProjectName, ProjectUID = Guid.NewGuid().ToString()},
        new Repositories.DBModels.Project() { Name = request.ProjectName, ProjectUID = request.ProjectUid.ToString()}
      };
      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(
        () => ProjectDataValidator.ValidateProjectName(_customerUid, projectName, request.ProjectUid.ToString(),
          log, ServiceExceptionHandler, projectRepo.Object));

      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2109", StringComparison.Ordinal), "Expected error number 2109");
      Assert.IsTrue(ex.GetContent.Contains("Not allowed duplicate, active projectnames: Count:2"), "should be 2 duplicate2");
    }
  }
}