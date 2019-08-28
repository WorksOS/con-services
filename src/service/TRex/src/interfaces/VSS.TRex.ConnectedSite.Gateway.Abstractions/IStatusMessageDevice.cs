using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace VSS.TRex.ConnectedSite.Gateway.Abstractions
{
  public interface IStatusMessageDevice
  {
    [JsonProperty("nickname")]
    string Nickname { get; set; }
    [JsonProperty("model")]
    string Model { get; set; }
    [JsonProperty("serialNumber")]
    string SerialNumber { get; set; }
    [JsonProperty("firmware")]
    string Firmware { get; set; }
    [JsonProperty("batteryPercent")]
    int? BatteryPercent { get; set; }
    [JsonProperty("licenseCodes")]
    string LicenseCodes { get; set; }
    [JsonProperty("swWarrantyExpUtc")]
    DateTime? WarrantyExpirationUtc { get; set; }
  }
}
