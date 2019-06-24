using System;
using System.Collections.Generic;
using System.Text;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.MasterData.Models.Models
{
  public class AssetDetails : IMasterDataModel
  {
    public string AssetId { get; set; }
    public string AssetUid { get; set; }
    public string AssetSerialNumber { get; set; }
    public string MakeCode { get; set; }
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public int AssetIcon { get; set; }
    public string ProductFamily { get; set; }
    public string Status { get; set; }
    public double HourMeter { get; set; }
    public double Odometer { get; set; }
    public double LastReportedLocationLatitude { get; set; }
    public double LastReportedLocationLongitude { get; set; }
    public string LastReportedLocation { get; set; }
    public DateTime LastReportedTimeUtc { get; set; }
    public DateTime LastLocationUpdateUtc { get; set; }
    public double FuelLevelLastReported { get; set; }
    public DateTime LastPercentFuelRemainingUtc { get; set; }
    public DateTime FuelReportedTimeUtc { get; set; }
    public double LifetimeFuel { get; set; }
    public DateTime LastLifetimeFuelLitersUtc { get; set; }
    public string CustomStateDescription { get; set; }
    public List<AssetDevice> Devices { get; set; }
    public string DealerName { get; set; }
    public string DealerCustomerNumber { get; set; }
    public string AccountName { get; set; }
    public string UniversalCustomerIdentifier { get; set; }
    public string UniversalCustomerName { get; set; }

    public List<string> GetIdentifiers()
    {
      return new List<string>() {AssetUid};
    }
  }
}
