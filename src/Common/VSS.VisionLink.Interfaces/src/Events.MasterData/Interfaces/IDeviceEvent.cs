using System;

namespace VSS.Visionlink.Interfaces.Events.MasterData.Interfaces
{
  public interface IDeviceEvent
  {
    Guid DeviceUID { get; set; }
    DateTime ActionUTC { get; set; }
  }
}
