using System;
using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus
{
  public class ConnectedDevice
  {
    [JsonProperty("nickname")]
    public string Nickname { get; set; }
    [JsonProperty("model")]
    public string Model { get; set; }
    [JsonProperty("serialNumber")]
    public string SerialNumber { get; set; }
    [JsonProperty("firmware")]
    public string Firmware { get; set; }
    [JsonProperty("batteryPercent")]
    public int? BatteryPercent { get; set; }
    [JsonProperty("licenseCodes")]
    public string LicenseCodes { get; set; }
    [JsonProperty("swWarrantyExpUtc")]
    public DateTime? WarrantyExpirationUtc { get; set; }

    public ConnectedDevice() { }
  }
}
