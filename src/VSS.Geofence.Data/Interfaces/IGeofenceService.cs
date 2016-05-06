using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace VSS.Geofence.Data.Interfaces
{
  public interface IGeofenceService
  {
    int StoreGeofence(IGeofenceEvent evt);
  }
}
