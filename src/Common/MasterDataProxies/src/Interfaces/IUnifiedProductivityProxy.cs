using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IUnifiedProductivityProxy : ICacheProxy
  {
    Task<List<GeofenceData>> GetAssociatedGeofences(string projectUid, IDictionary<string, string> customHeaders = null);
  }
}
