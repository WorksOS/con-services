using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Geofence.Data.Interfaces;

namespace VSS.Geofence.Data.Models
{
  public class CreateGeofenceEvent : IGeofenceEvent
  {
    public Guid CustomerUID { get; set; }
    public string GeofenceName { get; set; } //Required Field
    public string Description { get; set; }
    public string GeofenceType { get; set; } //Required Field
    public string GeometryWKT { get; set; }  //Required Field
    public int FillColor { get; set; }
    public bool IsTransparent { get; set; }
    public Guid GeofenceUID { get; set; }
    public Guid UserUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}
