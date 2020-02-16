using System;
using VSS.Hosted.VLCommon;

namespace VSS.Nighthawk.NHOPSvc.Interfaces.Models
{
  [Serializable]
  public class TireMonitoringStatus : INHOPDataObject
  {
    public long AssetID { get; set; }
    public string GPSDeviceID { get; set; }
    public DeviceTypeEnum DeviceType { get; set; }
    public long? SourceMsgID { get; set; }

    public bool isEnabled { get; set; }
  }
}
