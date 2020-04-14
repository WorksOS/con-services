using System;

namespace VSS.Visionlink.Interfaces.Events.MasterData.Interfaces
{
  public interface IGeofenceEvent
  {
    Guid GeofenceUID { get; set; }
    Guid UserUID { get; set; }
    DateTime ActionUTC { get; set; }
  }
}
