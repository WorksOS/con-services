using System;

namespace VSS.MasterData.Repositories.DBModels
{
  public class GeofenceWithAssociation
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

    public string ProjectUID { get; set; }

    public override bool Equals(object obj)
    {
      if (!(obj is GeofenceWithAssociation otherGeofenceWithAssociation))
      {
        return false;
      }

      return otherGeofenceWithAssociation.GeofenceUID == GeofenceUID
        && otherGeofenceWithAssociation.Name == Name
        && otherGeofenceWithAssociation.GeofenceType == GeofenceType
        && otherGeofenceWithAssociation.GeometryWKT == GeometryWKT
        && otherGeofenceWithAssociation.FillColor == FillColor
        && otherGeofenceWithAssociation.IsTransparent == IsTransparent
        && otherGeofenceWithAssociation.IsDeleted == IsDeleted
        && otherGeofenceWithAssociation.CustomerUID == CustomerUID
        && otherGeofenceWithAssociation.UserUID == UserUID
        && otherGeofenceWithAssociation.IsDeleted == IsDeleted
        && Math.Abs(otherGeofenceWithAssociation.AreaSqMeters - AreaSqMeters) < 0.0001
        && otherGeofenceWithAssociation.LastActionedUTC == LastActionedUTC
        && otherGeofenceWithAssociation.ProjectUID == ProjectUID;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}