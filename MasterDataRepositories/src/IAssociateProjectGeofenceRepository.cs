using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Repositories
{
  public interface IAssociateProjectGeofenceRepository
  {
    Task<AssociateProjectGeofence> GetAssociateProjectGeofence(string geofenceUid);
    Task<int> StoreEvent(IAssociateProjectGeofenceEvent evt);
    Task<IEnumerable<AssociateProjectGeofence>> GetAssociatedProjectGeofences(string projectUid);
  }
}