using System.Collections.Generic;
using VSS.Geofence.Data.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Geofence.Data.Interfaces
{
  public interface IGeofenceService
  {
    int StoreGeofence(IGeofenceEvent evt);
    IEnumerable<Models.Geofence> GetProjectGeofences(string customerUid);
    Models.Geofence GetGeofence(string geofenceUid);

  }
}
