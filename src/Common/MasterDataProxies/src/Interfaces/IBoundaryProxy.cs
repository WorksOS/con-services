using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IBoundaryProxy : ICacheProxy
  {
    Task<List<GeofenceData>> GetBoundaries(string projectUid, IDictionary<string, string> customHeaders = null);

    Task<GeofenceData> GetBoundaryForProject(string projectUid, string geofenceUid,
      IDictionary<string, string> customHeaders = null);

  }
}
