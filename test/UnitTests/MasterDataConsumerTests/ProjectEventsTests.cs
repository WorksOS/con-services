using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Project.Data;
using VSS.Project.Data.Models;

namespace Datafeed.Tests
{
  [TestClass]
  public class ProjectEventsTests
  {
    [TestMethod]
    public void ProjectEventsCopyModels()
    {
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var project = new Project()
      {
        ProjectUID = Guid.NewGuid().ToString(),
        LegacyProjectID = 12343,
        Name = "The Project Name",
        ProjectType = ProjectType.LandFill,
        IsDeleted = false,

        ProjectTimeZone = projectTimeZone,
        LandfillTimeZone = TimeZone.WindowsToIana(projectTimeZone),

        LastActionedUTC = now,
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(2017, 02, 01)
      };

      var kafkaProjectEvent = CopyModel(project);
      var copiedProject = CopyModel(kafkaProjectEvent);

      Assert.AreEqual(project, copiedProject, "Project model conversion not completed sucessfully");
    }

    private CreateProjectEvent CopyModel(Project project)
    {
      return new CreateProjectEvent()
      {
        ProjectUID = Guid.Parse(project.ProjectUID),
        ProjectID = project.LegacyProjectID,
        ProjectName = project.Name,
        ProjectType = project.ProjectType,
        ProjectTimezone = project.ProjectTimeZone,

        ProjectStartDate = project.StartDate,
        ProjectEndDate = project.EndDate,
        ActionUTC = project.LastActionedUTC
      };
    }

    private Project CopyModel(CreateProjectEvent kafkaProjectEvent)
    {
      return new Project()
      {
        ProjectUID = kafkaProjectEvent.ProjectUID.ToString(),
        LegacyProjectID = kafkaProjectEvent.ProjectID,
        Name = kafkaProjectEvent.ProjectName,
        ProjectType = kafkaProjectEvent.ProjectType,
        // IsDeleted =  N/A

        ProjectTimeZone = kafkaProjectEvent.ProjectTimezone,
        LandfillTimeZone = TimeZone.WindowsToIana(kafkaProjectEvent.ProjectTimezone),

        LastActionedUTC = kafkaProjectEvent.ActionUTC,
        StartDate = kafkaProjectEvent.ProjectStartDate,
        EndDate = kafkaProjectEvent.ProjectEndDate
      };
    }

  }
}