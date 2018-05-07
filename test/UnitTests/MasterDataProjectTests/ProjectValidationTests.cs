using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Moq;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ProjectValidationTests
  {
    protected ProjectErrorCodesProvider projectErrorCodesProvider = new ProjectErrorCodesProvider();
    private static List<Point> _boundaryLL;
    private static BusinessCenterFile _businessCenterFile;
    private static string _checkBoundaryString;

    private static string _customerUid;

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

      ProjectDataValidator.Validate(createProjectEvent, projectRepo.Object);
      request.CoordinateSystem = ProjectDataValidator.ValidateBusinessCentreFile(request.CoordinateSystem);
    }

    [TestMethod]
    public void ValidateCreateProjectV2Request_InvalidBoundary()
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
        () => ProjectDataValidator.Validate(createProjectEvent, projectRepo.Object));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2025", StringComparison.Ordinal), "Expected error number 2025");
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
  }
}