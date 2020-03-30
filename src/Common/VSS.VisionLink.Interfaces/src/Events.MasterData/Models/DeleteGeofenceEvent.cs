using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Models
{
  public class DeleteGeofenceEvent : IGeofenceEvent
  {
    public string GeofenceUID { get; set; }
    public string UserUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}
