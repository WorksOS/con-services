using System;
using VSS.Hosted.VLCommon;

namespace VSS.Nighthawk.NHOPSvc.Interfaces.Models
{
  [Serializable]
  public class AssetSecurityTamperResistanceStatus : INHOPDataObject
  {

    public long AssetID { get; set; }
    public string MakeCode { get; set; }
    public string SerialNumberVIN { get; set; }
    public string GPSDeviceID
    {
      get;
      set;
    }

    public DeviceTypeEnum DeviceType
    {
      get;
      set;
    }

    public long? SourceMsgID
    {
      get;
      set;
    }

    public DateTime? TimestampUtc { get; set; }
    public TamperResistanceStatus TamperLevel { get; set; }
    public string TamperConfigurationSource { get; set; }
  }
}
