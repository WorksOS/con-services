using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace Repositories.DBModels
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
    public string Description { get; set; }
    public string CustomerUID { get; set; }
    public string UserUID { get; set; }
    public DateTime LastActionedUTC { get; set; }

    public override bool Equals(object obj)
    {
      var otherGeofence = obj as Geofence;
      if (otherGeofence == null) return false;
      return otherGeofence.GeofenceUID == this.GeofenceUID            
            && otherGeofence.Name == this.Name
            && otherGeofence.GeofenceType == this.GeofenceType
            && otherGeofence.GeometryWKT == this.GeometryWKT
            && otherGeofence.FillColor == this.FillColor
            && otherGeofence.IsTransparent == this.IsTransparent
            && otherGeofence.IsDeleted == this.IsDeleted
            && otherGeofence.CustomerUID == this.CustomerUID
            && otherGeofence.UserUID == this.UserUID
            && otherGeofence.IsDeleted == this.IsDeleted
            && otherGeofence.LastActionedUTC == this.LastActionedUTC
            ;
    }
    public override int GetHashCode() { return 0; }
  }
}
