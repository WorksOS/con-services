using System;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Interfaces
{
  public interface IGeofenceEvent
  {
    Guid GeofenceUID { get; set; }
    string UserUID { get; set; }
    DateTime ActionUTC { get; set; }
    DateTime ReceivedUTC { get; set; }
  }
}
