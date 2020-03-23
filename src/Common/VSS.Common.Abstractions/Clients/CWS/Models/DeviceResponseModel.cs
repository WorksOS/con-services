using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class DeviceResponseModel
  {
    //Note: There are other properties returned but we only want some of it
    /// <summary>
    /// Device TRN ID
    /// </summary>
    [JsonProperty("deviceId")]
    public string Id { get; set; }

    /// <summary>
    /// deviceType
    /// </summary>
    [JsonProperty("deviceType")]
    public string DeviceType { get; set; }

    /// <summary>
    /// serial Number
    /// </summary>
    [JsonProperty("serialNumber")]
    public string SerialNumber { get; set; }
  }
}
