using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Repositories
{
  public interface IGeofenceRepository
  {
    Task<Geofence> GetGeofence(string geofenceUid);
    Task<int> StoreEvent(IGeofenceEvent evt);
    Task<IEnumerable<Geofence>> GetCustomerGeofences(string customerUid);
    Task<IEnumerable<Geofence>> GetGeofences(IEnumerable<string> geofenceUids);
  }
}
