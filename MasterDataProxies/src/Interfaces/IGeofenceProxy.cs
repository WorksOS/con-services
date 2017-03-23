using System.Collections.Generic;

namespace VSS.Raptor.Service.Common.Interfaces
{
  public interface IGeofenceProxy
  {
    string GetGeofenceBoundary(string geofenceUid, IDictionary<string, string> customHeaders = null);
  }
}
