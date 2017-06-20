using Repositories;
using Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace RepositoryTests.Internal
{
  public class TestHelpers
    {
    public static Project CopyModel(CreateProjectEvent kafkaProjectEvent)
    {
      return new Project
      {
        ProjectUID = kafkaProjectEvent.ProjectUID.ToString(),
        LegacyProjectID = kafkaProjectEvent.ProjectID,
        Name = kafkaProjectEvent.ProjectName,
        Description = kafkaProjectEvent.Description,
        ProjectType = kafkaProjectEvent.ProjectType,
        // IsDeleted =  N/A

        ProjectTimeZone = kafkaProjectEvent.ProjectTimezone,
        LandfillTimeZone = TimeZone.WindowsToIana(kafkaProjectEvent.ProjectTimezone),

        LastActionedUTC = kafkaProjectEvent.ActionUTC,
        StartDate = kafkaProjectEvent.ProjectStartDate,
        EndDate = kafkaProjectEvent.ProjectEndDate,
        GeometryWKT = kafkaProjectEvent.ProjectBoundary,
        CoordinateSystemFileName = kafkaProjectEvent.CoordinateSystemFileName
      };
    }
  }
}