using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon
{
  public class AssetDetail
  {
    public long AssetID { get; set; }
    public int DeviceTypeID { get; set; }
    public string SerialNumber { get; set; }
    public string MakeModel { get; set; }
    public double HourMeter { get; set; }
    public string LastUpdated { get; set; }
    public string UpdatedDate { get; set; }
    public double UpdatedHourMeter { get; set; }
    public string AssetName { get; set; }
    public int AssetIconID { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool LocIsValid { get; set; }
    public double OdoMeter { get; set; }
    public double UpdatedOdoMeter { get; set; }
        
  }
}