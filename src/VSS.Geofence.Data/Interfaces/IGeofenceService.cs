using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Geofence.Data.Interfaces
{
  public interface IGeofenceService
  {
    int StoreGeofence(IGeofenceEvent evt);
  }
}
