using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Models
{
  public class CreateDeviceEvent : IDeviceEvent
  {
    public string DeviceUID { get; set; }
    public int ShortRaptorAssetId { get; set; }
    public DateTime ActionUTC { get; set; }
  }
}
