using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Geofence.Data.Interfaces
{
  public interface IGeofenceEvent
  {
    Guid GeofenceUID { get; set; }
    Guid UserUID { get; set; }
    DateTime ActionUTC { get; set; }
    DateTime ReceivedUTC { get; set; }
  }
}
