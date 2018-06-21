using System;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// Describes geofence data returned by the geofence master data service.
  /// </summary>
  public class GeofenceData 
  {
    public string GeofenceName { get; set; }

    public string Description { get; set; }

    public string GeofenceType { get; set; }

    public string GeometryWKT { get; set; }

    public int FillColor { get; set; }
    
    public bool IsTransparent { get; set; }

    public Guid CustomerUID { get; set; }

    public Guid GeofenceUID { get; set; }

    public Guid UserUID { get; set; }

    public DateTime ActionUTC => DateTime.UtcNow;
    public double AreaSqMeters { get; set; }

    public override bool Equals(object obj)
    {
      // jcm: this is used by GeofenceProxy
      //       I intentionally didn't include geofenceUID or AreaSqMetres
      var otherGeofenceData = obj as GeofenceData;
      if (otherGeofenceData == null) return false;
      return otherGeofenceData.CustomerUID == this.CustomerUID
             && otherGeofenceData.GeofenceName == this.GeofenceName
             && otherGeofenceData.Description == this.Description
             && otherGeofenceData.GeofenceType == this.GeofenceType
             && otherGeofenceData.GeometryWKT == this.GeometryWKT
             && otherGeofenceData.FillColor == this.FillColor
             && otherGeofenceData.IsTransparent == this.IsTransparent
             && otherGeofenceData.UserUID == this.UserUID
        ;
    }
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
  }
}