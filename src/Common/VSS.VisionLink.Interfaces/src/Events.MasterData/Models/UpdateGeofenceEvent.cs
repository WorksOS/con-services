using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Models
{
  public class UpdateGeofenceEvent : IGeofenceEvent
  {
    public string GeofenceName { get; set; }
    public string Description { get; set; }
    public string GeofenceType { get; set; }
    public string GeometryWKT { get; set; }
    public int? FillColor { get; set; }
    public bool? IsTransparent { get; set; }
    public Guid GeofenceUID { get; set; }
    public Guid UserUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
    public DateTime? EndDate { get; set; }
	public double AreaSqMeters { get; set; }
  }
}
