using Newtonsoft.Json;
using System;
using VSS.MasterData.Models.Interfaces;

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

  }
}