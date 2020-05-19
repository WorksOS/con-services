using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class DeviceFromListResponseModel : IMasterDataModel
  {
    private string _trn;

    //Note: There are other properties returned but we only want some of it
    //   This is returned from a listDeviceFromAccount and returned different fields
    //      to say get deviceBySerial or deviceTRN
    /// <summary>
    /// Device TRN ID
    /// </summary>
    [JsonProperty("deviceId")]
    public string TRN
    {
      get => _trn;
      set
      {
        _trn = value;
        Id = TRNHelper.ExtractGuidAsString(value);
      }
    }

    /// <summary>
    /// WorksOS device ID; the Guid extracted from the TRN.
    /// </summary>
    public string Id { get; private set; }

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
    /// serial Number
    /// </summary>
    [JsonProperty("serialNumber")]
    public string SerialNumber { get; set; }

    /// <summary>
    /// relationStatus (sometimes referred to as accountRegistrationStatus) 
    /// </summary>
    [JsonProperty("accountRegistrationStatus")]
    public RelationStatusEnum RelationStatus { get; set; }

    /// <summary>
    /// tccDeviceStatus
    /// </summary>
    [JsonProperty("tccDeviceStatus")]
    public TCCDeviceStatusEnum TccDeviceStatus { get; set; }

    public List<string> GetIdentifiers() => new List<string> { TRN, Id };
  }
}
