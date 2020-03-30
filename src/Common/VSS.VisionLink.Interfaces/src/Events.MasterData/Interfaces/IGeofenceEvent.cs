using System;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces
{
  public interface IGeofenceEvent
  {
    Guid GeofenceUID { get; set; }
    string UserUID { get; set; }
    DateTime ActionUTC { get; set; }
    DateTime ReceivedUTC { get; set; }
  }
}
