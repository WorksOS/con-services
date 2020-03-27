using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  public class DeleteGeofenceEvent : IGeofenceEvent
  {
    public Guid GeofenceUID { get; set; }
    public string UserUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}
