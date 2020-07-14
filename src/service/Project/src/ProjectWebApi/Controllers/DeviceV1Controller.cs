using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Project.WebAPI.Common.Utilities;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Device controller v1
  ///   For first release, we'll mimic what WorksManager does:
  ///      "We'll only show devices that are "actively" reporting within the boundary of the project.
  ///           Once they step outside of the boundary, they'll still be associated with the project but won't show on the map.
  ///           I say "active" as I believe we remove them from the map if they haven't reported in the last 30 days"
  ///  CWS by default restricts devices to those claimed, and associated with the project.
  /// </summary>
  public class DeviceV1Controller : BaseController<DeviceV1Controller>
  {
    /// <summary>
    /// Gets list of devices with last known status (LKS) for this project
    ///    We impose these rules for the UI:
    ///      a) only devices reporting within the last 30 days
    ///      b) restrict data to only that of interest to UI (at present): lat/long/deviceName/DeviceType/SerialNumber/LastReportedUTC
    ///      possible future: restrict to devices of interest to WorksOS (EC and CB)
    /// </summary>
    /// <returns>
    ///  NotFound means that the endpoint was not found. if no devices or project not found, will return an empty list.
    /// </returns>
    [HttpGet("api/v1/devices")]
    public async Task<IActionResult> GetDevicesLKSForProject(
      [FromQuery] Guid projectUid,
      [FromQuery] DateTime? earliestOfInterestUtc)
    {
      Logger.LogInformation($"{nameof(GetDevicesLKSForProject)} projectUid {projectUid} earliestOfInterestUtc {earliestOfInterestUtc}");
      DeviceDataValidator.ValidateProjectUid(projectUid);
      DeviceDataValidator.ValidateEarliestOfInterestUtc(earliestOfInterestUtc);

      earliestOfInterestUtc ??= DateTime.UtcNow.AddDays(-30);
      var devices = await CwsDeviceGatewayClient.GetDevicesLKSForProject(projectUid, earliestOfInterestUtc, customHeaders);

      Logger.LogInformation($"{nameof(GetDevicesLKSForProject)} completed. devices {(devices == null ? null : JsonConvert.SerializeObject(devices))}");
      return Ok(devices);
    }

    /// <summary>
    /// Gets device with last known status (LKS).
    ///    Is UI interested in what project the device is currently on? 
    ///     i.e. UI gets list for project, but device moves off the project (can tell from ProjectName - include this for now)
    /// </summary>
    [HttpGet("api/v1/device")]
    public async Task<IActionResult> GetDeviceWithLKS([FromQuery] string deviceName)
    {
      Logger.LogInformation($"{nameof(GetDeviceWithLKS)} deviceName {deviceName}");
      DeviceDataValidator.ValidateDeviceName(deviceName);

      var device = await CwsDeviceGatewayClient.GetDeviceLKS(deviceName, customHeaders);
      Logger.LogInformation($"{nameof(GetDeviceWithLKS)} completed. device {(device == null ? null : JsonConvert.SerializeObject(device))}");
      if (device == null)
        return NotFound();

      return Ok(device);
    }
  }
}
