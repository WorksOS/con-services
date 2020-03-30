using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Models
{
  public class CreateGeofenceEvent : IGeofenceEvent
  {
    public string CustomerUID { get; set; }
    public string GeofenceName { get; set; } //Required Field
    public string Description { get; set; }
    public string GeofenceType { get; set; } //Required Field
    public string GeometryWKT { get; set; }  //Required Field
    public int FillColor { get; set; }
    public bool IsTransparent { get; set; }
    public Guid GeofenceUID { get; set; }
    public string UserUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
    public DateTime? EndDate { get; set; }
    public double AreaSqMeters { get; set; }
  }
}
