using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Hosted.VLCommon;

namespace VSS.Nighthawk.NHOPSvc.Interfaces.Models
{
  [Serializable]
  public class DailyReportFrequency : INHOPDataObject
  {
    public long AssetID { get; set; }
    public string GPSDeviceID { get; set; }
    public DeviceTypeEnum DeviceType { get; set; }
    public long? SourceMsgID { get; set; }
    public int ReportFrequency { get; set; }
  }
}
