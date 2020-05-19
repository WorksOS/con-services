using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Project.WebAPI.Internal;
using VSS.Productivity3D.AssetMgmt3D.Abstractions.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Device controller v1
  /// </summary>
  public class DeviceV1Controller : ProjectBaseController
  {
    //TODO: Is this mock data still used ?

    /// <summary>
    /// Get location data for a given set of Devices.
    /// </summary>
    [HttpPost("api/v1/device/location")]
    public IActionResult GetDeviceLocationData([FromBody] List<Guid> deviceIds)
    {
      var deviceIdsDisplay = string.Join(", ", deviceIds ?? new List<Guid>());
      Logger.LogInformation($"{nameof(GetDeviceLocationData)} Getting Device location data for: {deviceIdsDisplay}");

      var assets = MockDeviceRepository.GetAssets(deviceIds);

      var resultSet = new List<AssetLocationData>(assets.Count);

      foreach (var asset in assets)
      {
        resultSet.Add(new AssetLocationData
        {
          AssetUid = Guid.Parse(asset.AssetUID),
          AssetIdentifier = asset.EquipmentVIN,
          AssetSerialNumber = asset.SerialNumber,
          AssetType = asset.AssetType,
          LocationLastUpdatedUtc = asset.LastActionedUtc,
          MachineName = asset.Name,
          Latitude = 0,
          Longitude = 0,
        });
      }

      Logger.LogInformation($"Returning location data for {resultSet.Count} Assets.");
      return Json(resultSet);
    }
  }
}
