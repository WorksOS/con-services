using System;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Events.MasterData.Models
{
  public class CreateDeviceEvent : IDeviceEvent
  {
    public Guid DeviceUID { get; set; }
    public int ShortRaptorAssetID { get; set; }
    public DateTime ActionUTC { get; set; }
  }
}
