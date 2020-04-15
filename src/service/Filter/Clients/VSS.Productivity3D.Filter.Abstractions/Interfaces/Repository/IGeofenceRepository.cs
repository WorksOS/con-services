using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;

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
