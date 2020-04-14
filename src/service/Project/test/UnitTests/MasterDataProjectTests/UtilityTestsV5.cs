using System;
using System.Collections.Generic;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.ProjectTests
{
  public class UtilityTestsV5
  {
    private static List<TBCPoint> _boundaryLL;
    private static BusinessCenterFile _businessCenterFile;
    private static string _checkBoundaryString;
    private static string _customerUid;

    public UtilityTestsV5()
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
        FileSpaceId = "u710e3466-1d47-45e3-87b8-81d1127ed4ed",
        Path = "/BC Data/Sites/Chch Test Site",
        Name = "CTCTSITECAL.dc",
        CreatedUtc = DateTime.UtcNow.AddDays(-0.5)
      };

      _customerUid = Guid.NewGuid().ToString();
    }

    [Fact]
    public void MapCreateProjectV5RequestToEvent()
    {
      var requestedProjectType = ProjectType.Standard;
      var expectedProjectType = ProjectType.Standard;
      var request = CreateProjectV5Request.CreateACreateProjectV5Request
        (requestedProjectType, new DateTime(2017, 01, 20), new DateTime(2017, 02, 15), "projectName",
        "New Zealand Standard Time", _boundaryLL, _businessCenterFile);
      var creatProjectEvent = MapV5Models.MapCreateProjectV5RequestToEvent(request, _customerUid);

      Assert.Equal(Guid.Empty, creatProjectEvent.ProjectUID);
      Guid.TryParse(creatProjectEvent.CustomerUID.ToString(), out var customerUidOut);
      Assert.Equal(_customerUid, customerUidOut.ToString());
      Assert.Equal(expectedProjectType, creatProjectEvent.ProjectType);
      Assert.Equal(request.ProjectName, creatProjectEvent.ProjectName);
      Assert.Null(creatProjectEvent.Description);
      Assert.Equal(request.ProjectStartDate, creatProjectEvent.ProjectStartDate);
      Assert.Equal(request.ProjectEndDate, creatProjectEvent.ProjectEndDate);
      Assert.Equal(request.ProjectTimezone, creatProjectEvent.ProjectTimezone);
      Assert.Equal(_checkBoundaryString, creatProjectEvent.ProjectBoundary);
      Assert.Equal(_businessCenterFile.Name, creatProjectEvent.CoordinateSystemFileName);
      Assert.True(creatProjectEvent.ActionUTC > DateTime.MinValue, "ActionUTC has not been mapped correctly");
    }

    [Fact]
    public void MapProjectToV4Result()
    {
      var project = new ProjectDatabaseModel
      {
        ProjectUID = Guid.NewGuid().ToString(),
        ShortRaptorProjectId = 123,
        ProjectType = ProjectType.Standard,
        Name = "the Name",
        Description = "the Description",
        ProjectTimeZone = "NZ stuff",
        ProjectTimeZoneIana = "Pacific stuff",
        StartDate = new DateTime(2017, 01, 20),
        EndDate = new DateTime(2017, 02, 15),
        CustomerUID = Guid.NewGuid().ToString(),
        Boundary = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))",
        CoordinateSystemFileName = "",
        CoordinateSystemLastActionedUTC = new DateTime(2017, 01, 21),

        IsArchived = false,
        LastActionedUTC = new DateTime(2017, 01, 21)
      };

      var result = AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project);
      Assert.Equal(project.ProjectUID, result.ProjectUid);
      Assert.Equal(project.ShortRaptorProjectId, result.ShortRaptorProjectId);
      Assert.Equal(project.ProjectType, result.ProjectType);
      Assert.Equal(project.Name, result.Name);
      Assert.Equal(project.Description, result.Description);
      Assert.Equal(project.ProjectTimeZone, result.ProjectTimeZone);
      Assert.Equal(project.ProjectTimeZoneIana, result.IanaTimeZone);
      Assert.Equal(project.StartDate.ToString("O"), result.StartDate);
      Assert.Equal(project.EndDate.ToString("O"), result.EndDate);
      Assert.Equal(project.CustomerUID, result.CustomerUid);
      Assert.Equal(project.Boundary, result.ProjectGeofenceWKT);
      Assert.False(result.IsArchived, "IsArchived has not been mapped correctly");

      // just make a copy
      var copyOfProject = AutoMapperUtility.Automapper.Map<ProjectDatabaseModel>(project);
      Assert.Equal(project.ProjectUID, copyOfProject.ProjectUID);
      Assert.Equal(project.ShortRaptorProjectId, copyOfProject.ShortRaptorProjectId);
    }

    [Fact]
    public void MapProjectToV5Result()
    {
      var project = new ProjectDatabaseModel
      {
        ProjectUID = Guid.NewGuid().ToString(),
        ShortRaptorProjectId = 123,
        ProjectType = ProjectType.Standard,
        Name = "the Name",
        Description = "the Description",
        ProjectTimeZone = "NZ stuff",
        ProjectTimeZoneIana = "Pacific stuff",
        StartDate = new DateTime(2017, 01, 20),
        EndDate = new DateTime(2017, 02, 15),
        CustomerUID = Guid.NewGuid().ToString(),
        Boundary = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))",
        CoordinateSystemFileName = "",
        CoordinateSystemLastActionedUTC = new DateTime(2017, 01, 21),

        IsArchived = false,
        LastActionedUTC = new DateTime(2017, 01, 21)
      };

      var result = AutoMapperUtility.Automapper.Map<ProjectV5DescriptorResult>(project);
      Assert.Equal(project.ShortRaptorProjectId, result.ShortRaptorProjectId);
      Assert.Equal(project.Name, result.Name);
      Assert.Equal(project.StartDate.ToString("O"), result.StartDate);
      Assert.Equal(project.EndDate.ToString("O"), result.EndDate);
      Assert.Equal(project.ProjectType, result.ProjectType);
    }

  }
}
