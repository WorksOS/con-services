using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceStatusContext
  {
    public DeviceAssetDto DeviceAsset { get; set; }
    public string IBKey { get; set; }
    public string Status { get; set; }
    public DateTime ActionUTC { get; set; } 

    public DeviceStatusContext()
    {
      DeviceAsset = new DeviceAssetDto();
    }
  }
}
