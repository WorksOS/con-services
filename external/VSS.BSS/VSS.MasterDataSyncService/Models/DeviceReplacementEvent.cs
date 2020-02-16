using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class DeviceReplacementEvent:IDeviceEvent
  {
    public Guid OldDeviceUID { get; set; }
    public Guid NewDeviceUID { get; set; }
    public Guid AssetUID { get; set; }
    public DateTime ActionUTC { get; set; }
  }
}
