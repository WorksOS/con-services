using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IUnifiedProductivityProxy
  {
    Task<List<GeofenceData>> GetAssociatedGeofences(string projectUid);
  }
}
