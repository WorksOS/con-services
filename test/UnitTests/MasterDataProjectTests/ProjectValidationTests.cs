using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ProjectValidationTests :ExecutorBaseTests
  {
    protected ProjectErrorCodesProvider projectErrorCodesProvider = new ProjectErrorCodesProvider();
    private static List<Point> _boundaryLL;
    private static BusinessCenterFile _businessCenterFile;
    private static string _checkBoundaryString;
    private readonly string _validBoundary;
    private readonly string _invalidBoundary;
    private static string _customerUid;
    

    public ProjectValidationTests()
    {
      _validBoundary =  "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))";
      _invalidBoundary = "blah";
    }

  [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
      _boundaryLL = new List<Point>()
      {
        new Point(-43.5, 172.6),
        new Point(-43.5003, 172.6),
        new Point(-43.5003, 172.603),
        new Point(-43.5, 172.603)
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
      Assert.AreEqual(_checkBoundaryString, createProjectEvent.ProjectBoundary,"Invalid ProjectBoundary in WKT");

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.ProjectExists(It.IsAny<string>())).ReturnsAsync(false);

      ProjectDataValidator.Validate(createProjectEvent, projectRepo.Object, ServiceExceptionHandler);
      request.CoordinateSystem = ProjectDataValidator.ValidateBusinessCentreFile(request.CoordinateSystem);
    }

    [TestMethod]
    public void ValidateCreateProjectV2Request_BoundaryTooFewPoints()
    {
      var invalidBoundaryLL = new List<Point>()
      {
        new Point(-43.5, 172.6)
      };

      var request = CreateProjectV2Request.CreateACreateProjectV2Request
      (ProjectType.Standard, new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "projectName",
        "New Zealand Standard Time", invalidBoundaryLL, _businessCenterFile);
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
      Assert.AreEqual("/BC Data/Sites/Chch Test Site", resultantBusinessCenterFile.Path,"Path should have bounding slashes inserted");

      bcf = BusinessCenterFile.CreateBusinessCenterFile(_businessCenterFile.FileSpaceId, _businessCenterFile.Path,
        _businessCenterFile.Name, _businessCenterFile.CreatedUtc);
      bcf.Path = "";
      var ex = Assert.ThrowsException < ServiceException >(() =>  ProjectDataValidator.ValidateBusinessCentreFile(bcf));
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

  }
}