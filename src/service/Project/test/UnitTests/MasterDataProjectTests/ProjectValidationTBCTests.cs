using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Exceptions;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models;
using Xunit;

namespace VSS.MasterData.ProjectTests
{
  public class ProjectValidationTBCTests : UnitTestsDIFixture<ProjectValidationTBCTests>
  {
    private static List<TBCPoint> _boundaryLL;
    private static BusinessCenterFile _businessCenterFile;
    private static string _checkBoundaryString;
    private readonly string _validBoundary =
      "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))";
    private readonly string _invalidBoundary = "blah";

    public ProjectValidationTBCTests()
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
    }

    [Fact]
    public void ValidateCreateProjectV5Request_HappyPath()
    {
      var request = CreateProjectV5Request.CreateACreateProjectV5Request
      (CwsProjectType.AcceptsTagFiles, new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "projectName",
        "New Zealand Standard Time", _boundaryLL, _businessCenterFile);
      var createProjectEvent = MapV5Models.MapCreateProjectV5RequestToEvent(request, _customerUid.ToString());
      Assert.Equal(_checkBoundaryString, createProjectEvent.ProjectBoundary);

      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      ProjectDataValidator.Validate(createProjectEvent, _customerUid, _userUid, _log, ServiceExceptionHandler, cwsProjectClient.Object, _customHeaders);
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
      (CwsProjectType.AcceptsTagFiles, new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "projectName",
        "New Zealand Standard Time", invalidBoundaryLl, _businessCenterFile);
      var createProjectEvent = MapV5Models.MapCreateProjectV5RequestToEvent(request, _customerUid.ToString());

      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var ex = Assert.Throws<ServiceException>(
        () => ProjectDataValidator.Validate(createProjectEvent, _customerUid, _userUid, _log, ServiceExceptionHandler, cwsProjectClient.Object, _customHeaders));
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
      (CwsProjectType.AcceptsTagFiles, new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "projectName",
        "New Zealand Standard Time", invalidBoundaryLl, _businessCenterFile);
      var createProjectEvent = MapV5Models.MapCreateProjectV5RequestToEvent(request, _customerUid.ToString());

      var project = CreateProjectDetailModel(_customerTrn, _projectTrn);
      var cwsProjectClient = new Mock<ICwsProjectClient>();
      cwsProjectClient.Setup(ps => ps.GetMyProject(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IHeaderDictionary>())).ReturnsAsync(project);

      var ex = Assert.Throws<ServiceException>(
        () => ProjectDataValidator.Validate(createProjectEvent, _customerUid, _userUid, _log, ServiceExceptionHandler, cwsProjectClient.Object, _customHeaders));
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
  }
}
