using System;

namespace VSS.MasterData.Models.Models
{
  public class GeofenceDescriptor : IModelDescriptor
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

    public override bool Equals(object obj)
    {
      var geofenceDescriptor = obj as GeofenceDescriptor;
      if (geofenceDescriptor == null)
      {
        return false;
      }

      return geofenceDescriptor.GeofenceUID == GeofenceUID
             && geofenceDescriptor.GeofenceName == GeofenceName
             && geofenceDescriptor.GeometryWKT == GeometryWKT;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}