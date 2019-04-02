using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.Models.Models
{
  public class AssetStatus
  {
    public string AssetIdentifier { get; set; }
    public string AssetSerialNumber { get; set; }
    public string MakeCode { get; set; }
    public string Model { get; set; }
    public string Manufacturer { get; set; }
    public int AssetIcon { get; set; }
    public string Status { get; set; }
    public double HourMeter { get; set; }
    public double LastReportedLocationLatitude { get; set; }
    public double LastReportedLocationLongitude { get; set; }
    public string LastReportedLocation { get; set; }
    public int FuelLevelLastReported { get; set; }
    public DateTime LastReportedTime { get; set; }
    public DateTime LastLocationUpdateUTC { get; set; }
  }
}
