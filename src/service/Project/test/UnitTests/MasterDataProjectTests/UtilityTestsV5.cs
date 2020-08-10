using System;
using System.Collections.Generic;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Extensions;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
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
      var expectedProjectType = CwsProjectType.AcceptsTagFiles;
      var request = CreateProjectV5Request.CreateACreateProjectV5Request("projectName", _boundaryLL, _businessCenterFile);
      var createProjectEvent = MapV5Models.MapCreateProjectV5RequestToEvent(request, _customerUid);

      Assert.Equal(Guid.Empty, createProjectEvent.ProjectUID);
      Guid.TryParse(createProjectEvent.CustomerUID.ToString(), out var customerUidOut);
      Assert.Equal(_customerUid, customerUidOut.ToString());
      Assert.Equal(expectedProjectType, createProjectEvent.ProjectType);
      Assert.Equal(request.ProjectName, createProjectEvent.ProjectName);
      Assert.Equal(_checkBoundaryString, createProjectEvent.ProjectBoundary);
      Assert.Equal(_businessCenterFile.Name, createProjectEvent.CoordinateSystemFileName);
      Assert.True(createProjectEvent.ActionUTC > DateTime.MinValue, "ActionUTC has not been mapped correctly");
    }

    [Fact]
    public void MapProjectToV6Result()
    {
      var project = new ProjectDatabaseModel
      {
        ProjectUID = Guid.NewGuid().ToString(),
        ProjectType = CwsProjectType.AcceptsTagFiles,
        Name = "the Name",
        ProjectTimeZone = "NZ stuff",
        ProjectTimeZoneIana = "Pacific stuff",
        CustomerUID = Guid.NewGuid().ToString(),
        Boundary = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))",
        CoordinateSystemFileName = "",
        CoordinateSystemLastActionedUTC = new DateTime(2017, 01, 21),

        IsArchived = false,
        LastActionedUTC = new DateTime(2017, 01, 21)
      };

      var result = AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project);
      Assert.Equal(project.ProjectUID, result.ProjectUid);
      Assert.Equal(new Guid(project.ProjectUID).ToLegacyId(), result.ShortRaptorProjectId);
      Assert.Equal(project.ProjectType, result.ProjectType);
      Assert.Equal(project.Name, result.Name);
      Assert.Equal(project.ProjectTimeZone, result.ProjectTimeZone);
      Assert.Equal(project.ProjectTimeZoneIana, result.IanaTimeZone);
      Assert.Equal(project.CustomerUID, result.CustomerUid);
      Assert.Equal(project.Boundary, result.ProjectGeofenceWKT);
      Assert.False(result.IsArchived, "IsArchived has not been mapped correctly");

      // just make a copy
      var copyOfProject = AutoMapperUtility.Automapper.Map<ProjectDatabaseModel>(project);
      Assert.Equal(project.ProjectUID, copyOfProject.ProjectUID);
      Assert.Equal(project.ShortRaptorProjectId, copyOfProject.ShortRaptorProjectId);
    }
  }
}
