using System;

namespace VSS.MasterData.Repositories.DBModels
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
    public double AreaSqMeters { get; set; }
    public DateTime LastActionedUTC { get; set; }

    public override bool Equals(object obj)
    {
      var otherGeofence = obj as Geofence;
      if (otherGeofence == null)
      {
        return false;
      }

      return otherGeofence.GeofenceUID == GeofenceUID
        && otherGeofence.Name == Name
        && otherGeofence.GeofenceType == GeofenceType
        && otherGeofence.GeometryWKT == GeometryWKT
        && otherGeofence.FillColor == FillColor
        && otherGeofence.IsTransparent == IsTransparent
        && otherGeofence.IsDeleted == IsDeleted
        && otherGeofence.CustomerUID == CustomerUID
        && otherGeofence.UserUID == UserUID
        && otherGeofence.IsDeleted == IsDeleted
        && Math.Abs(otherGeofence.AreaSqMeters - AreaSqMeters) < 0.0001
        && otherGeofence.LastActionedUTC == LastActionedUTC;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}