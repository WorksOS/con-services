using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VSS.MasterData.Proxies.Interfaces
{
    public interface IGeofenceProxy
    {
        Task<string> GetGeofenceBoundary(string geofenceUid, IDictionary<string, string> customHeaders = null);

        Task<Guid> CreateGeofence(Guid customerGuid, string geofenceName, string description, string geofenceType,
            string geometryWKT, int fillColor, bool isTransparent, Guid userUid,
            IDictionary<string, string> customHeaders = null);

    }
}
