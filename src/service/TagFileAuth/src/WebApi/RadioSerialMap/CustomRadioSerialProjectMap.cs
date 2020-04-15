using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.RadioSerialMap
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

    private string key(string radioSerial, string radioType) => $"{radioSerial}|{radioType}";

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
      // {map:[{radioSerial:string, radioType:string, assetId:string, assetUid:string, projectId:string, projectUid:string}]}

      if (fileContent.map == null)
      {
        _log.LogError("No map element found in the root of the json map file");
        return;
      }

      foreach (var elem in fileContent.map)
      {
        if (elem.radioSerial == null || elem.radioType == null)
        {
          _log.LogError($"Either/both of radioSerial and radioType not present in mapping element: {elem}");
        }

        if (!_map.TryAdd(key(elem.radioSerial, elem.radioType),
          new RadioSerialMapAssetIdentifier()
          {
            assetId = elem.assetId ?? "", assetUid = elem.assetId ?? "", projectId = elem.projectId ?? "", projectUid = elem.projectUid ?? "",
          }))
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
    /// <param name="radioType"></param>
    /// <param name="radioSerialMapAssetIdentifier"></param>
    /// <returns></returns>
    public bool LocateAsset(string radioSerial, string radioType, out RadioSerialMapAssetIdentifier radioSerialMapAssetIdentifier)
    {
      if (_map.TryGetValue(key(radioSerial, radioType), out var value))
      {
        radioSerialMapAssetIdentifier = value;
        return true;
      }

      radioSerialMapAssetIdentifier = NullRadioSerialMapAssetIdentifier;
      return false;
    }
  }
}
