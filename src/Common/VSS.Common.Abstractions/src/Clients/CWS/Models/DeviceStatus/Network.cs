using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus
{
  public class Network
  {
    [JsonProperty("type")]
    public string Type { get; set; }
    [JsonProperty("signal")] 
    public double? Signal { get; set; }
    [JsonProperty("state")] 
    public string State { get; set; }
    [JsonProperty("dbm")]
    public short? Dbm { get; set; }
    [JsonProperty("uptime")]
    public short? Uptime { get; set; }
    [JsonProperty("carrier")]
    public string Carrier { get; set; }
    [JsonProperty("roaming")]
    public bool? Roaming { get; set; }
    [JsonProperty("cellTech")]
    public string CellTech { get; set; }
    [JsonProperty("mcc")]
    public string Mcc { get; set; }
    [JsonProperty("mnc")]
    public string Mnc { get; set; }
    [JsonProperty("simId")]
    public string SimId { get; set; }
    [JsonProperty("iccId")]
    public string IccId { get; set; }
    [JsonProperty("phoneNumber")]
    public string PhoneNumber { get; set; }
    [JsonProperty("modemResets")]
    public long? ModemResets { get; set; }
    [JsonProperty("apn")]
    public string Apn { get; set; }
    [JsonProperty("apnUsername")]
    public string ApnUsername { get; set; }
    [JsonProperty("apnPasswordSet")]
    public bool? ApnPasswordSet { get; set; }
    [JsonProperty("txData")]
    public short? TxData { get; set; }
    [JsonProperty("rxData")]
    public short? RxData { get; set; }
    [JsonProperty("regulatoryDomain")]
    public string RegulatoryDomain { get; set; }
    [JsonProperty("regulatoryDomainMethod")]
    public string RegulatoryDomainMethod { get; set; }

    public Network() { }
  }
}
