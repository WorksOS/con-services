using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Repositories.DBModels;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.Filter.Abstractions.Interfaces.Repository
{
  public interface IGeofenceRepository
  {
    Task<Geofence> GetGeofence(string geofenceUid);
    Task<int> StoreEvent(IGeofenceEvent evt);
    Task<IEnumerable<Geofence>> GetCustomerGeofences(string customerUid);
    Task<IEnumerable<Geofence>> GetGeofences(IEnumerable<string> geofenceUids);
  }
}
