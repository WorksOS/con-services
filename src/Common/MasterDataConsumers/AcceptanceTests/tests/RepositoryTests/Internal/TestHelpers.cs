using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
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
        LandfillTimeZone = PreferencesTimeZones.WindowsToIana(kafkaProjectEvent.ProjectTimezone),

        LastActionedUTC = kafkaProjectEvent.ActionUTC,
        StartDate = kafkaProjectEvent.ProjectStartDate,
        EndDate = kafkaProjectEvent.ProjectEndDate,
        GeometryWKT = kafkaProjectEvent.ProjectBoundary,
        CoordinateSystemFileName = kafkaProjectEvent.CoordinateSystemFileName
      };
    }

    public static bool CompareSubs(Subscription original, Subscription result)
    {
      return original.SubscriptionUID == result.SubscriptionUID
             && original.CustomerUID == result.CustomerUID
             && original.ServiceTypeID == result.ServiceTypeID
             && original.StartDate == result.StartDate
             && original.EndDate == result.EndDate
             && original.LastActionedUTC == result.LastActionedUTC;
    }
  }
}