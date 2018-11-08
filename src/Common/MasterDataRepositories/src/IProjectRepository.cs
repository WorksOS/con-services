using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Repositories
{
  public interface IProjectRepository
  {
    Task<bool> ProjectExists(string projectUid);
    Task<bool> CustomerProjectExists(string projectUid);
    Task<ImportedFile> GetImportedFile(string importedFileUid);
    Task<IEnumerable<ImportedFile>> GetImportedFiles(string projectUid);
    Task<Project> GetProject(long legacyProjectID);
    Task<Project> GetProject(string projectUid);
    Task<IEnumerable<Project>> GetProjectAndSubscriptions(long legacyProjectID, DateTime validAtDate);
    Task<Project> GetProjectBySubcription(string subscriptionUid);
    Task<Project> GetProjectOnly(string projectUid);
    Task<ProjectSettings> GetProjectSettings(string projectUid, string userId, ProjectSettingsType projectSettingsType);
    Task<IEnumerable<ProjectSettings>> GetProjectSettings(string projectUid, string userId);
    Task<IEnumerable<Project>> GetProjectsForCustomer(string customerUid);
    Task<IEnumerable<Project>> GetProjectsForCustomerUser(string customerUid, string userUid);
    Task<IEnumerable<Project>> GetProjectsForUser(string userUid);
    Task<IEnumerable<ProjectGeofence>> GetAssociatedGeofences(string projectUid);
    Task<IEnumerable<GeofenceWithAssociation>> GetCustomerGeofences(string customerUid);
    Task<IEnumerable<Project>> GetProjects_UnitTests();
    Task<Project> GetProject_UnitTest(string projectUid);


    Task<bool> DoesPolygonOverlap(string customerUid, string geometryWkt, DateTime startDate,
      DateTime endDate, string excludeProjectUid = "");
    Task<IEnumerable<Project>> GetStandardProject(string customerUID, double latitude, double longitude,
      DateTime timeOfPosition);
    Task<IEnumerable<Project>> GetProjectMonitoringProject(string customerUID, double latitude, double longitude,
      DateTime timeOfPosition, int projectType, int serviceType);
    Task<IEnumerable<Project>> GetIntersectingProjects(string customerUid, double latitude, double longitude,
      int[] projectTypes, DateTime? timeOfPosition = null);
    
    Task<int> StoreEvent(IProjectEvent evt);
  }
}