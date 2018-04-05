using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
    public interface IGeofenceProxy :ICacheProxy
    {
      Task<string> GetGeofenceBoundary(string customerUid, string geofenceUid, IDictionary<string, string> customHeaders = null);

      Task<Guid> CreateGeofence(Guid customerGuid, string geofenceName, string description, string geofenceType,
          string geometryWKT, int fillColor, bool isTransparent, Guid userUid, double areaSqMeters,
          IDictionary<string, string> customHeaders = null);

      Task<List<GeofenceData>> GetGeofences(string customerUid, IDictionary<string, string> customHeaders = null);

      Task<GeofenceData> GetGeofenceForCustomer(string customerUid, string geofenceUid,
        IDictionary<string, string> customHeaders = null);

    }
}
