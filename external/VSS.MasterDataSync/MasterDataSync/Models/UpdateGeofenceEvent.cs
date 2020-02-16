using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class UpdateGeofenceEvent:IGeofenceEvent
  {
    public string GeofenceName { get; set; }
    public string Description { get; set; }
    public string GeofenceType { get; set; }
    public string GeometryWKT { get; set; }
    public int? FillColor { get; set; }
    public bool? IsTransparent { get; set; }
    public Guid? UserUID { get; set; }
    public Guid GeofenceUID { get; set; }
    public DateTime ActionUTC { get; set; }
  }
}
