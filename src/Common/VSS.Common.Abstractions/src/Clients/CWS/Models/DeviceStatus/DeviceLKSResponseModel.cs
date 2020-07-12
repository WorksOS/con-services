using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Enums;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus
{
  ///
  /// Do we need isLlhSiteLocal for lat/lon grid conversion?
  ///
  /// If this is the result of a getDeviceLKS for Project,
  ///  a) projectName should match that requested.
  ///  b) the list of projectTRNs should ONLY include the project requested.
  ///  There is an issue on staging, where cws can list other projects and the name can be different.
  ///   Apparently, it IS the correct device, only the names are wrong.
  ///  This shouldn't happen in production.
  public class DeviceLKSResponseModel : IMasterDataModel
  {
    private string _deviceTrn;

    /// <summary>
    /// Device TRN ID
    /// </summary>
    [JsonProperty("deviceId")]
    public string TRN
    {
      get => _deviceTrn;
      set
      {
        _deviceTrn = value;
        deviceUid = TRNHelper.ExtractGuidAsString(value);
      }
    }

    /// <summary>
    /// WorksOS device ID; the Guid extracted from the TRN.
    /// </summary>
    public string deviceUid { get; private set; }

    /// <summary>
    /// deviceName
    ///     "AssetType"-"serialNumber"
    ///     e.g. "TABLET-12345YU" 
    /// </summary>
    public string deviceName { get; set; }

    public double? lat { get; set; }

    public double? lon { get; set; }

    /// <summary>
    /// The type of device running the earthworks software
    ///    e.g. TABLET EC520 etc
    ///    this is not the 'asset' type i.e. excavator
    ///       nor the radio type e.g. Torch
    /// </summary>
    [JsonConverter(typeof(NullableEnumStringConverter), CWSDeviceTypeEnum.Unknown)]
    public CWSDeviceTypeEnum assetType { get; set; }

    /// <summary>
    /// projectName
    ///     should be project which the device is currently on
    ///     could return the projectId which is in the attached list..
    ///       ... wait to see what the requirements are.
    /// </summary>
    public string projectName { get; set; }

    public DateTime? lastReported { get; set; }

    /// <summary>
    /// assetSerialNumber
    ///     e.g. "12345YU" 
    /// </summary>
    public string assetSerialNumber { get; set; }


    public string designName { get; set; }
    public string designId { get; set; }
    public string deviceNickname { get; set; }
    public string swWarrantyExpUtc { get; set; }
    public string osName { get; set; }
    public string appName { get; set; }
    public string correctionSource { get; set; }
    public string ts { get; set; }
    public double? h { get; set; }
    public string operatorName { get; set; }
    public bool? isLlhSiteLocal { get; set; }
    public string language { get; set; }
    public string coordinateSystemHash { get; set; }
    public bool? isDataLogging { get; set; }
    public string antennaType { get; set; }
    public string targetType { get; set; }
    public double? rodHeight { get; set; }
    public short? radioIntegrity { get; set; }
    public string systemStatus { get; set; }
    public string attachmentName { get; set; }
    public string attachmentWearUpdateUtc { get; set; }
    public string workOrderName { get; set; }
    public string designType { get; set; }
    public string designSurfaceName { get; set; }
    public double? designVertOffset { get; set; }
    public double? designPerpOffset { get; set; }
    public string assetNickname { get; set; }
    public string assetMake { get; set; }
    public string assetModel { get; set; }
    public string osVersion { get; set; }
    public string appVersion { get; set; }
    public double? freeSpace { get; set; }
    public short? batteryPercent { get; set; }
    public string powerSource { get; set; }
    public string licenseCodes { get; set; }
    public string baseStationName { get; set; }
    public double? baseStationLat { get; set; }
    public double? baseStationLon { get; set; }
    public double? baseStationHeight { get; set; }
    public short? internalTemp { get; set; }
    public short? totalRunTime { get; set; }
    public short? totalCellTime { get; set; }
    public short? totalWifiTime { get; set; }
    public short? totalAppTime { get; set; }
    public short? totalAutomaticsTime { get; set; }

    private string _accountTrn;

    public string accountId
    {
      get => _accountTrn;
      set
      {
        _accountTrn = value;
        accountUid = TRNHelper.ExtractGuidAsString(value);
      }
    }

    public string accountUid { get; private set; }

    public List<Network> networks { get; set; }
    public List<GNSSAntenna> gnss { get; set; }
    public List<ConnectedDevice> devices { get; set; }
    public List<ProjectId> projects { get; set; }
    public List<string> GetIdentifiers() => new List<string> { TRN, deviceUid };
  }
}

/* example
[
  {
    "deviceId": "trn::profilex:us-west-2:device:08d629d9-d66c-f80b-dbc2-ff00010003a5",
    "deviceName": "Tablet-DELL-79W74M2",
    "lat": 39.89666361833241,
    "lon": -105.11382752518381,
    "designName": "Parking Lot Amal.V05",
    "designId": "Parking Lot Amal.V05",
    "deviceNickname": null,
    "assetType": "Tablet",
    "projectName": "Trimble Building Phase II",
    "lastReported": "2020-04-29T07:02:57Z",
    "assetSerialNumber": "DELL-79W74M2",
    "swWarrantyExpUtc": "9999-12-31",
    "osName": "Win32NT",
    "appName": "Trimble Siteworks",
    "correctionSource": "IBSS",
    "ts": "2020-04-29T07:02:55Z",
    "h": 1643.860439463545,
    "operatorName": "Siteworks User",
    "isLlhSiteLocal": null,
    "language": "en-US",
    "coordinateSystemHash": null,
    "isDataLogging": null,
    "antennaType": "Trimble SPS855",
    "targetType": null,
    "rodHeight": 2.000101600203201,
    "radioIntegrity": null,
    "systemStatus": null,
    "attachmentName": null,
    "attachmentWearUpdateUtc": null,
    "workOrderName": "1",
    "designType": null,
    "designSurfaceName": "Parking Lot Amal.V05",
    "designVertOffset": 0,
    "designPerpOffset": 0,
    "assetNickname": "Tablet-SCS900-1234",
    "assetMake": null,
    "assetModel": null,
    "osVersion": "Microsoft Windows NT 10.0.14393.0",
    "appVersion": "1.11.19302.221",
    "freeSpace": null,
    "batteryPercent": null,
    "powerSource": null,
    "licenseCodes": null,
    "baseStationName": null,
    "baseStationLat": null,
    "baseStationLon": null,
    "baseStationHeight": null,
    "internalTemp": null,
    "totalRunTime": null,
    "totalCellTime": null,
    "totalWifiTime": null,
    "totalAppTime": null,
    "totalAutomaticsTime": null,
    "networks": null,
    "gnss": null,
    "devices": null,
    "projects": [
        {
            "projectId": "trn::profilex:us-west-2:project:1978f53e-14a4-48d2-a33c-dfb2e10a0e68"
        }
    ],
    "accountId": "trn::profilex:us-west-2:account:67c9f1e4-3e9b-4f94-9428-f4ed30d83d76"
  }
 */
