using System;

namespace VSS.Geofence.Data.Models
{
  public class Geofence
  {
    public string geofenceUid { get; set; }
    public string name { get; set; }
    public GeofenceType geofenceType { get; set; }
    public string geometryWKT { get; set; }
    public int? fillColor { get; set; }
    public bool? isTransparent { get; set; }
    public bool isDeleted { get; set; }
    public string customerUid { get; set; }
    public string projectUid { get; set; }
    public DateTime lastActionedUtc { get; set; }
  }
}
