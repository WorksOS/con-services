using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class DissociateDeviceAssetEvent:IDeviceEvent
  {
    public Guid DeviceUID { get; set; }
    public Guid AssetUID { get; set; }
    public DateTime ActionUTC { get; set; }
  }
}
