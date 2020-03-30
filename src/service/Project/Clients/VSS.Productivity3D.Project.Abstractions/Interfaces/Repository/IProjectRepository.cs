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

    #region projects

    Task<Models.DatabaseModels.Project> GetProject(string projectUid);
    
    Task<Models.DatabaseModels.Project> GetProject(long shortRaptorProjectId);
    
    Task<Models.DatabaseModels.Project> GetProjectOnly(string projectUid);

    Task<bool> ProjectExists(string projectUid);

    Task<IEnumerable<Models.DatabaseModels.Project>> GetProjectsForCustomer(string customerUid);

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

    #region geofenceForFilters  // this geofence code is used by FilterSvc and refer to tables solely in the FilterSvc database (not the ProjectSvc one).

    Task<IEnumerable<ProjectGeofence>> GetAssociatedGeofences(string projectUid);

    Task<IEnumerable<bool>> DoPolygonsOverlap(string projectGeometryWkt, IEnumerable<string> geometryWkts);
    
    #endregion geofenceForFilters

  }
}
