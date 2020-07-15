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
  public class DeviceLKSResponseModel : DeviceLKSModel, IMasterDataModel
  {
    private string _deviceTrn;

    /// <summary>
    /// Device TRN ID
    /// </summary>
    [JsonProperty("deviceId")]
    public string DeviceTrn
    {
      get => _deviceTrn;
      set
      {
        _deviceTrn = value;
        DeviceUid = TRNHelper.ExtractGuidAsString(value);
      }
    }

    /// <summary>
    /// WorksOS deviceUid; the Guid extracted from the TRN.
    /// </summary>
    [JsonProperty("deviceUid")]
    public string DeviceUid { get; private set; }

    /// <summary>
    /// deviceName
    ///     "AssetType"-"serialNumber"
    ///     e.g. "TABLET-12345YU" 
    /// </summary>
    [JsonProperty("deviceName")]
    public string DeviceName { get; set; }
    [JsonProperty("deviceNickname")] 
    public string DeviceNickname { get; set; }

    [JsonProperty("designId")]
    public string DesignId { get; set; }

    [JsonProperty("lastReported")]
    public DateTime? LastReportedUtc { get; set; }

    private string _accountTrn;

    [JsonProperty("accountId")]
    public string AccountTrn
    {
      get => _accountTrn;
      set
      {
        _accountTrn = value;
        AccountUid = TRNHelper.ExtractGuidAsString(value);
      }
    }

    [JsonProperty("accountUid")]
    public string AccountUid { get; private set; }
    
    public List<ProjectId> projects { get; set; }
    public List<string> GetIdentifiers() => new List<string> { DeviceTrn, DeviceUid };
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
