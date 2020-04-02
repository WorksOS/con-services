using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces.Repository
{
  public interface IProjectRepository
  {
    #region projectstore

    Task<int> StoreEvent(IProjectEvent evt);

    #endregion projectstore

    #region projectsov

    Task<Models.DatabaseModels.Project> GetProject(string projectUid);
    
    Task<Models.DatabaseModels.Project> GetProject(long shortRaptorProjectId);
    
    Task<Models.DatabaseModels.Project> GetProjectOnly(string projectUid);

    Task<bool> ProjectExists(string projectUid);

    Task<IEnumerable<Models.DatabaseModels.Project>> GetProjectsForCustomer(string customerUid);

    Task<Models.DatabaseModels.Project> GetProject_UnitTests(string projectUid);

    Task<IEnumerable<Models.DatabaseModels.Project>> GetProjectHistory_UnitTests(string projectUid);

    #endregion projects

    #region projectSpatial

    Task<bool> DoesPolygonOverlap(string customerUid, string geometryWkt, DateTime startDate,
      DateTime endDate, string excludeProjectUid = "");
    
    Task<IEnumerable<Models.DatabaseModels.Project>> GetIntersectingProjects(string customerUid, double latitude, double longitude,
      DateTime? timeOfPosition = null);
    
    #endregion projectSpatial

    #region projectSettings

    Task<ProjectSettings> GetProjectSettings(string projectUid, string userId, ProjectSettingsType projectSettingsType);
    
    Task<IEnumerable<ProjectSettings>> GetProjectSettings(string projectUid, string userId);

    #endregion projectSettings
    
    #region importedFiles

    Task<ImportedFile> GetImportedFile(string importedFileUid);

    Task<IEnumerable<ImportedFile>> GetReferencedImportedFiles(string importedFileUid);

    Task<IEnumerable<ImportedFile>> GetImportedFiles(string projectUid);

    #endregion importedFiles

    // this geofence code is used by projectSvc and FilterSvc and wll refer to database within the service
    #region geofenceForFilters  

    Task<IEnumerable<ProjectGeofence>> GetAssociatedGeofences(string projectUid);

    Task<IEnumerable<bool>> DoPolygonsOverlap(string projectGeometryWkt, IEnumerable<string> geometryWkts);
    
    #endregion geofenceForFilters

  }
}
