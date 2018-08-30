using System;

namespace Common.Models
{
  public class Geofence
  {
    public string GeofenceUID { get; set; }
    public string Name { get; set; }
    public GeofenceType GeofenceType { get; set; }
    public string GeometryWKT { get; set; }
    public int? FillColor { get; set; }
    public bool? IsTransparent { get; set; }
    public bool IsDeleted { get; set; }
    public string CustomerUID { get; set; }
    public string ProjectUID { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }
}