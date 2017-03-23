using System;
using System.Collections.Generic;

namespace VSS.Raptor.Service.Common.Interfaces
{
    public interface IGeofenceProxy
    {
        string GetGeofenceBoundary(string geofenceUid, IDictionary<string, string> customHeaders = null);

        Guid CreateGeofence(Guid customerGuid, string geofenceName, string description, string geofenceType,
            string geometryWKT, int fillColor, bool isTransparent, Guid userUid,
            IDictionary<string, string> customHeaders = null);

    }
}
