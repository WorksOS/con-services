using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class DeviceTransferEvent:IDeviceEvent
  {
    public Guid DeviceUID { get; set; }
    public Guid OldAssetUID { get; set; }
    public Guid NewAssetUID { get; set; }
    public DateTime ActionUTC { get; set; }
  }
}
