using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Enums;

namespace VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus
{
  ///
  /// This includes the core components to a create LKS and a response
  /// 
  public class DeviceLKSModel 
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
    /// project which the device is currently on
    /// </summary>
    [JsonProperty("projectName")]
    public string ProjectName { get; set; }

    /// <summary>
    /// assetSerialNumber
    ///     e.g. "12345YU" 
    /// </summary>
    [JsonProperty("assetSerialNumber")] 
    public string AssetSerialNumber { get; set; }

    /// <summary>
    /// The type of asset the device is running on
    ///    e.g. Tablet or for EC/CB then grader; excavator
    ///    this is not the platformType e.g. EC520 or radio type e.g. Torch
    ///    see Trex  enum MachineType for those included in tagFiles
    /// </summary>
    [JsonProperty("assetType")]
    public string AssetType { get; set; }

    [JsonProperty("assetNickname")]
    public string AssetNickname { get; set; }
    [JsonProperty("assetMake")] 
    public string AssetMake { get; set; }
    [JsonProperty("assetModel")] 
    public string AssetModel { get; set; }

    [JsonProperty("designName")]
    public string DesignName { get; set; }
    [JsonProperty("designType")]
    public string DesignType { get; set; }
    [JsonProperty("designSurfaceName")]
    public string DesignSurfaceName { get; set; }
    [JsonProperty("designVertOffset")]
    public double? DesignVertOffset { get; set; }
    [JsonProperty("designPerpOffset")]
    public double? DesignPerpOffset { get; set; }

    [JsonProperty("correctionSource")]
    public string CorrectionSource { get; set; }
    [JsonProperty("operatorName")]
    public string OperatorName { get; set; }
    [JsonProperty("isLlhSiteLocal")]
    public bool? IsLlhSiteLocal { get; set; }
    [JsonProperty("language")]
    public string Language { get; set; }
    [JsonProperty("coordinateSystemHash")]
    public string CoordinateSystemHash { get; set; }
    [JsonProperty("isDataLogging")]
    public bool? IsDataLogging { get; set; }
    [JsonProperty("antennaType")]
    public string AntennaType { get; set; }
    [JsonProperty("targetType")]
    public string TargetType { get; set; }
    [JsonProperty("rodHeight")]
    public double? RodHeight { get; set; }
    [JsonProperty("radioIntegrity")]
    public short? RadioIntegrity { get; set; }
    [JsonProperty("systemStatus")]
    public string SystemStatus { get; set; }
    [JsonProperty("attachmentName")]
    public string AttachmentName { get; set; }
    [JsonProperty("attachmentWearUpdateUtc")]
    public string AttachmentWearUpdateUtc { get; set; }
    [JsonProperty("workOrderName")]
    public string WorkOrderName { get; set; }

    [JsonProperty("osName")]
    public string OsName { get; set; }
    [JsonProperty("osVersion")]
    public string OsVersion { get; set; }

    [JsonProperty("appName")]
    public string AppName { get; set; }
    [JsonProperty("appVersion")]
    public string AppVersion { get; set; }

    [JsonProperty("freeSpace")]
    public double? FreeSpace { get; set; }
    [JsonProperty("batteryPercent")]
    public short? BatteryPercent { get; set; }
    [JsonProperty("powerSource")]
    public string PowerSource { get; set; }
    [JsonProperty("licenseCodes")]
    public string LicenseCodes { get; set; }
    [JsonProperty("swWarrantyExpUtc")]
    public string SwWarrantyExpUtc { get; set; }

    [JsonProperty("baseStationName")]
    public string baseStationName { get; set; }
    [JsonProperty("baseStationLat")]
    public double? baseStationLat { get; set; }
    [JsonProperty("baseStationLon")]
    public double? baseStationLon { get; set; }
    [JsonProperty("baseStationHeight")]
    public double? baseStationHeight { get; set; }
    [JsonProperty("internalTemp")]
    public short? internalTemp { get; set; }
    [JsonProperty("totalRunTime")]
    public short? totalRunTime { get; set; }
    [JsonProperty("totalCellTime")]
    public short? totalCellTime { get; set; }
    [JsonProperty("totalWifiTime")]
    public short? totalWifiTime { get; set; }
    [JsonProperty("totalAppTime")]
    public short? totalAppTime { get; set; }
    [JsonProperty("totalAutomaticsTime")]
    public short? totalAutomaticsTime { get; set; }

    [JsonProperty("networks")]
    public List<Network> Networks { get; set; }
    [JsonProperty("gnss")]
    public List<GNSSAntenna> Gnss { get; set; }
    [JsonProperty("devices")]
    public List<ConnectedDevice> Devices { get; set; }
  }
}
