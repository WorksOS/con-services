using System;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Events.MasterData.Models
{
  public class DeleteGeofenceEvent : IGeofenceEvent
  {
    public Guid GeofenceUID { get; set; }
    public Guid UserUID { get; set; }
    public DateTime ActionUTC { get; set; }
  }
}
