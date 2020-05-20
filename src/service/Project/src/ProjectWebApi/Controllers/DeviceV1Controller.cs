using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
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

    /// <summary>
    /// Gets list of devices with last known status (LKS).
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="lastReported"></param>
    /// <returns></returns>
    [HttpGet("api/v1/devices")]
    public IActionResult GetDevicesWithLKS(
      [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? lastReported)
    {
      var logMsg = $"{nameof(GetDevicesWithLKS)} Getting list of devices with last known status (LKS)";
      var project = projectUid != Guid.Empty ? $"for: {projectUid}" : "";
      var lastReportedDate = lastReported != null ? $"at: {lastReported}" : "";

      Logger.LogInformation($"{logMsg} {project} {lastReportedDate}.");

      var devices = MockDeviceRepository.GetDevicesWithLKS();

      logMsg = "Returning list of devices with last known status (LKS) data";

      Logger.LogInformation($"{logMsg} {project} {lastReportedDate}.");

      return Json(devices);
    }

    /// <summary>
    /// Gets device with last known status (LKS).
    /// </summary>
    /// <param name="deviceName"></param>
    /// <returns></returns>
    [HttpGet("api/v1/device")]
    public IActionResult GetDeviceWithLKS([FromQuery] string deviceName)
    {
      Logger.LogInformation($"{nameof(GetDeviceWithLKS)} Getting device with last known status (LKS). Device name: {deviceName}");

      DeviceDataValidator.ValidateDeviceName(deviceName);

      var device = MockDeviceRepository.GetDeviceWithLKS();

      Logger.LogInformation($"Returning device with last known status (LKS). Device name: {deviceName}.");

      return Json(device);
    }
  }
}
