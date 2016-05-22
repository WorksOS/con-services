using System.Collections.Generic;
using VSS.Geofence.Data.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Geofence.Data.Interfaces
{
  public interface IGeofenceService
  {
    int StoreGeofence(IGeofenceEvent evt);
    Models.Geofence GetGeofenceByName(string customerUid, string name);
    int AssignGeofenceToProject(string geofenceUid, string projectUid);
    void AssignApplicableLandfillGeofencesToProject(string projectGeometry, string customerUid, string projectUid);
    IEnumerable<Models.Geofence> GetProjectGeofences(string customerUid);
    GeofenceType GetGeofenceType(IGeofenceEvent evt);

  }
}
