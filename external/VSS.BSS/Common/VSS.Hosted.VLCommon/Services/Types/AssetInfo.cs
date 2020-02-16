using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon.Services.Types
{
   public class AssetInfo
  {
    public long assetID;
    public long fk_DeviceID;
    public string ownerBssID;
    public string assetSerialNumber;
    public string assetName;
    public string make;
    public string makeCode;
    public string model;
    public string makeModel { get { return string.Format("{0} {1}", make, model).Trim(); } }
    public string productFamily;
    public int deviceTypeID;
    public int lastStateID;
    public double? hourMeterValueNullable;
    public double? odometerValueNullable;
    public double hourMeterValue { get { return !hourMeterValueNullable.HasValue ? double.NaN : Math.Round(hourMeterValueNullable.Value); } }
    public double odometerValue { get { return !odometerValueNullable.HasValue ? double.NaN : Math.Round(odometerValueNullable.Value); } }
    public string location;
    public DateTime LastReportedUTC;
    public string gpsDeviceID;
  }
}
