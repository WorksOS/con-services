using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces.Repository
{
  public interface IProjectRepository
  {
    #region projectstore

    Task<int> StoreEvent(IProjectEvent evt);

    #endregion projectstore
    
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

    #endregion geofenceForFilters

  }
}
