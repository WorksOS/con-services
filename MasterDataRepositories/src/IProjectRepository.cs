using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Repositories
{
  public interface IProjectRepository
  {
    Task<bool> CustomerProjectExists(string projectUid);
    Task<bool> DoesPolygonOverlap(string customerUID, string geometryWKT, DateTime startDate, DateTime endDate);
    Task<ImportedFile> GetImportedFile(string importedFileUid);
    Task<IEnumerable<ImportedFile>> GetImportedFiles(string projectUid);
    Task<Project> GetProject(long legacyProjectID);
    Task<Project> GetProject(string projectUid);
    Task<IEnumerable<Project>> GetProjectAndSubscriptions(long legacyProjectID, DateTime validAtDate);
    Task<Project> GetProjectBySubcription(string subscriptionUid);

    Task<IEnumerable<Project>> GetProjectMonitoringProject(string customerUID, double latitude, double longitude,
      DateTime timeOfPosition, int projectType, int serviceType);

    Task<Project> GetProjectOnly(string projectUid);
    Task<ProjectSettings> GetProjectSettings(string projectUid);
    Task<IEnumerable<Project>> GetProjectsForCustomer(string customerUid);
    Task<IEnumerable<Project>> GetProjectsForCustomerUser(string customerUid, string userUid);
    Task<IEnumerable<Project>> GetProjectsForUser(string userUid);
    Task<IEnumerable<Project>> GetProjects_UnitTests();
    Task<Project> GetProject_UnitTest(string projectUid);

    Task<IEnumerable<Project>> GetStandardProject(string customerUID, double latitude, double longitude,
      DateTime timeOfPosition);

    Task<bool> ProjectExists(string projectUid);
    Task<int> StoreEvent(IProjectEvent evt);
  }
}