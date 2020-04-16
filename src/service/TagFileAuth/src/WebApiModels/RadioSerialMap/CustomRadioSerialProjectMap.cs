using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.RadioSerialMap
{
  /// <summary>
  /// This defines a set of specific mappings of radio serial/type asset identification to projects to support TMC and other special cases that do not have devices
  /// provisioned in a standard or discoverable way
  /// </summary>
  public class CustomRadioSerialProjectMap : ICustomRadioSerialProjectMap
  {
    private readonly ILogger _log;

    /// <summary>
    /// Name of the device to asset/project ID mapping file
    /// </summary>
    public static string DeviceToAssetProjectMapFileName = "devicetoassetprojectmap.json";

    /// <summary>
    /// Constructor...
    /// </summary>
    /// <param name="logger"></param>
    public CustomRadioSerialProjectMap(ILoggerFactory logger)
    {
      _log = logger.CreateLogger<CustomRadioSerialProjectMap>();

      LoadDictionary();
    }

    private string key(string radioSerial, int deviceType) => $"{radioSerial}|{deviceType}";

    private readonly Dictionary<string, RadioSerialMapAssetIdentifier> _map = new Dictionary<string, RadioSerialMapAssetIdentifier>();

    /// <summary>
    /// Loads a JSON map containing the radio serial/type -> Asset ID/UID and Project ID/UID
    /// It assumes a file called DeviceToAssetProjectMap.json exists co-located with the running binary
    /// </summary>
    private void LoadDictionary()
    {
      if (!File.Exists(DeviceToAssetProjectMapFileName))
      {
        _log.LogInformation($"Device to asset/project mapping file {DeviceToAssetProjectMapFileName} not found");
      }

      dynamic fileContent = JsonConvert.DeserializeObject(File.ReadAllText(DeviceToAssetProjectMapFileName));

      // JSON file is of schema:
      // {map:[{radioSerial:string, deviceType:string, assetId:string, assetUid:string, projectId:string, projectUid:string}]}

      if (fileContent.map == null)
      {
        _log.LogError("No map element found in the root of the json map file");
        return;
      }

      foreach (var elem in fileContent.map)
      {
        if (elem.radioSerial == null || elem.deviceType == null)
        {
          _log.LogError($"Either/both of radioSerial and deviceType not present in mapping element: {elem}");
          continue;
        }

        var newItem = new RadioSerialMapAssetIdentifier
        {
          AssetId = Convert.ToInt64(elem.assetId?.Value ?? "-1"),
          AssetUid = new Guid(elem.assetUid?.Value ?? ""),
          ProjectId = Convert.ToInt64(elem.projectId?.Value ?? "-1"),
          ProjectUid = new Guid(elem.projectUid?.Value ?? "")
        };

        if (!((newItem.AssetId != -1 && newItem.ProjectId != -1) ||
              (newItem.AssetUid.CompareTo(Guid.Empty) != 0) && newItem.ProjectUid.CompareTo(Guid.Empty) != 0))
        {
          _log.LogError($"Asset and project IDs/UIDs not set correctly in mapping element: {elem}");
          continue;
        }

        if (!_map.TryAdd(key(elem.radioSerial.Value, Convert.ToInt32(elem.deviceType.Value)), newItem))
        {
          _log.LogError($"Radio device to asset/project map already contains an entry for {elem}");
        }
      }

      _log.LogInformation($"{_map.Count} radio serial/type -> asset/project mappings read");
    }

    private static readonly RadioSerialMapAssetIdentifier NullRadioSerialMapAssetIdentifier = new RadioSerialMapAssetIdentifier();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="radioSerial"></param>
    /// <param name="deviceType"></param>
    /// <param name="radioSerialMapAssetIdentifier"></param>
    /// <returns></returns>
    public bool LocateAsset(string radioSerial, int deviceType, out RadioSerialMapAssetIdentifier radioSerialMapAssetIdentifier)
    {
      if (_map.TryGetValue(key(radioSerial, deviceType), out var value))
      {
        radioSerialMapAssetIdentifier = value;
        return true;
      }

      radioSerialMapAssetIdentifier = NullRadioSerialMapAssetIdentifier;
      return false;
    }
  }
}
