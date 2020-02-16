using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceAssetContext
  {
    public DeviceAssetDto OldDeviceAsset { get; set; }
    public DeviceAssetDto NewDeviceAsset { get; set; }
    public string OldIBKey { get; set; }
    public string NewIBKey { get; set; }
    public DateTime ActionUTC { get; set; }
    public long SequenceNumber { get; set; }

    public DeviceAssetContext()
    {
      OldDeviceAsset = new DeviceAssetDto();
      NewDeviceAsset = new DeviceAssetDto();
    }
  }

  public class DeviceAssetDto
  {
    public long DeviceId { get; set; }
    public string IbKey { get; set; }
    public string GpsDeviceId { get; set; }
    public DeviceTypeEnum? Type { get; set; }
    public string OwnerBSSID { get; set; }
    public DeviceStateEnum DeviceState { get; set; }
    
    public long AssetId { get; set; }
    public string Name { get; set; }
    public DateTime? InsertUTC { get; set; }
    public StoreEnum AssetStore { get; set; }
    public string AssetSerialNumber { get; set; }
    public string AssetMakeCode { get; set; }
    public bool AssetExists { get { return AssetId > 0; } }
    public bool DeviceExists { get { return DeviceId > 0; } }
  }
}
