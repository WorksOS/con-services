using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Geofence.Data.Interfaces;

namespace VSS.Geofence.Data.Models
{
  public class DeleteGeofenceEvent : IGeofenceEvent
  {
    public Guid GeofenceUID { get; set; }
    public Guid UserUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}
