using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class DeviceResponseModel : IMasterDataModel
  {
    //Note: There are other properties returned but we only want some of it
    /// <summary>
    /// Device TRN ID
    /// </summary>
    [JsonProperty("deviceId")]
    public string Id { get; set; }

    /// <summary>
    /// Account TRN ID
    /// </summary>
    [JsonProperty("accountId")]
    public string AccountId { get; set; }

    /// <summary>
    /// deviceType
    /// </summary>
    [JsonProperty("deviceType")]
    public string DeviceType { get; set; }

    /// <summary>
    /// deviceName
    /// </summary>
    [JsonProperty("deviceName")]
    public string DeviceName { get; set; }

    /// <summary>
    /// status (ACTIVE etal?)
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; }

    /// <summary>
    /// serial Number
    /// </summary>
    [JsonProperty("serialNumber")]
    public string SerialNumber { get; set; }

    public List<string> GetIdentifiers() => new List<string> { Id };
  }
}
