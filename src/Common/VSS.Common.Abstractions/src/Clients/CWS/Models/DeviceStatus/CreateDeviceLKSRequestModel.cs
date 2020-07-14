using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Enums;

namespace VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus
{
  ///
  /// We only want to send location, not the other LKS stuff,
  ///     so this model is restricted for that purpose
  public class CreateDeviceLKSRequestModel
  {

    [JsonProperty("ts")]
    public DateTime? TimeStamp { get; set; }

    [JsonProperty("lat")]
    public double? Latitude { get; set; }

    [JsonProperty("lon")] 
    public double? Longitude { get; set; }

    [JsonProperty("h")]
    public double? Height { get; set; }

    /// <summary>
    /// assetSerialNumber
    ///     e.g. "12345YU" 
    /// </summary>
    [JsonProperty("assetSerialNumber")] 
    public string SerialNumber { get; set; }

    /// <summary>
    /// The type of device running the earthworks software
    ///    e.g. TABLET EC520 etc
    ///    this is not the 'asset' type i.e. excavator
    ///       nor the radio type e.g. Torch
    /// </summary>
    [JsonProperty("assetType")] 
    [JsonConverter(typeof(NullableEnumStringConverter), CWSDeviceTypeEnum.Unknown)]
    public CWSDeviceTypeEnum DeviceType { get; set; }

    [JsonProperty("assetNickname")]
    public string AssetNickname { get; set; }

    [JsonProperty("designName")] 
    public string DesignName { get; set; }

    [JsonProperty("appName")]
    [DefaultValue("GCS900")]  
    public string AppName { get; set; }

    [JsonProperty("appVersion")]
    public string AppVersion { get; set; }


    [JsonProperty("devices")]
    public List<ConnectedDevice> Devices { get; set; }
  }
}
