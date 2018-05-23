using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.MasterDataConsumer.Tests
{
  [TestClass]
  public class ProjectEventsTests
  {
    DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);
    string projectTimeZone = "New Zealand Standard Time";
    string boundaryWKT = "POLYGON((172.582309 -43.545285,172.582309 43.545239,172.582443 43.545239,172.582443 -43.545285))";
    Project project = null;

    const string polygonStr = "POLYGON";

    [TestInitialize]   
    public void TestInitialize()
    {
      project = new Project()
      {
        ProjectUID = Guid.NewGuid().ToString(),
        LegacyProjectID = 12343,
        Name = "The Project Name",
        ProjectType = ProjectType.LandFill,
        IsDeleted = false,

        ProjectTimeZone = projectTimeZone,
        LandfillTimeZone = PreferencesTimeZones.WindowsToIana(projectTimeZone),

        LastActionedUTC = now,
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(2017, 02, 01),
        GeometryWKT = boundaryWKT
      };
    }

    [TestMethod]
    public void ProjectEventsCopyModels()
    {
      var kafkaProjectEvent = CopyModel(project);
      var copiedProject = CopyModel(kafkaProjectEvent);

      Assert.AreEqual(copiedProject.GeometryWKT, boundaryWKT, "Project's boundary conversion not completed sucessfully");
      Assert.AreNotEqual(project, copiedProject, "Project model conversion not completed sucessfully");
    }

    private CreateProjectEvent CopyModel(Project project)
    {
      // Check whether the GeometryWKT is in WKT format. Convert to the old format if it is. 
      if (project.GeometryWKT.Contains(polygonStr))
        project.GeometryWKT = project.GeometryWKT.Replace(polygonStr + "((", "").Replace("))", "").Replace(',', ';').Replace(' ', ',') + ';';

      return new CreateProjectEvent()
      {
        ProjectUID = Guid.Parse(project.ProjectUID),
        ProjectID = project.LegacyProjectID,
        ProjectName = project.Name,
        ProjectType = project.ProjectType,
        ProjectTimezone = project.ProjectTimeZone,

        ProjectStartDate = project.StartDate,
        ProjectEndDate = project.EndDate,
        ProjectBoundary = project.GeometryWKT,
        ActionUTC = project.LastActionedUTC
      };
    }

    private Project CopyModel(CreateProjectEvent kafkaProjectEvent)
    {
      // Check whether the ProjectBoundary is in WKT format. Convert to the WKT format if it is not. 
      if (!kafkaProjectEvent.ProjectBoundary.Contains(polygonStr)) {
        kafkaProjectEvent.ProjectBoundary = kafkaProjectEvent.ProjectBoundary.Replace(',', ' ').Replace(';', ',').TrimEnd(',');        
        kafkaProjectEvent.ProjectBoundary = String.Concat(polygonStr + "((", kafkaProjectEvent.ProjectBoundary, "))");
      }

      return new Project()
      {
        ProjectUID = kafkaProjectEvent.ProjectUID.ToString(),
        LegacyProjectID = kafkaProjectEvent.ProjectID,
        Name = kafkaProjectEvent.ProjectName,
        ProjectType = kafkaProjectEvent.ProjectType,
        // IsDeleted =  N/A

        ProjectTimeZone = kafkaProjectEvent.ProjectTimezone,
        LandfillTimeZone = PreferencesTimeZones.WindowsToIana(kafkaProjectEvent.ProjectTimezone),

        LastActionedUTC = kafkaProjectEvent.ActionUTC,
        StartDate = kafkaProjectEvent.ProjectStartDate,
        GeometryWKT = kafkaProjectEvent.ProjectBoundary,
        EndDate = kafkaProjectEvent.ProjectEndDate
      };
    }

  }
}