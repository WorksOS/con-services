using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockProjectDeviceController : BaseController
  {

    public MockProjectDeviceController(ILoggerFactory loggerFactory)
    : base(loggerFactory)
    {
    }

    [HttpGet("api/v1/device/applicationcontext/serialnumber")]
    public DeviceDataSingleResult GetDeviceBySerialNumber([FromQuery] string serialNumber)
    {
      Logger.LogInformation($"{nameof(GetDeviceBySerialNumber)} serialNumber {serialNumber}");
      return new DeviceDataSingleResult();
    }

    [HttpGet("api/v1/device/applicationcontext/shortRaptorAssetId")]
    public DeviceDataSingleResult GetDevice([FromQuery] int shortRaptorAssetId)
    {
      Logger.LogInformation($"{nameof(GetDevice)} serialNumber {shortRaptorAssetId}");
      return new DeviceDataSingleResult();
    }

    [HttpGet("api/v1/device/applicationcontext/{deviceUid}/projects")]
    public ProjectDataResult GetProjectsForDevice(string deviceUid)
    {
      Logger.LogInformation($"{nameof(GetProjectsForDevice)} deviceUid {deviceUid}");
      return new ProjectDataResult();
    }


    [HttpPost("api/v1/devices/deviceuids")]
    [ProducesResponseType(typeof(List<DeviceMatchingModel>), 200)]
    public IActionResult GetMatchingAssets([FromBody] List<Guid> deviceUids)
    {
      Logger.LogInformation($@"Get MockAssetResolverList for supplied Guids: {deviceUids}");

      return Ok(new DeviceMatchingModel { deviceIdentifiers = new List<KeyValuePair<Guid, long>>() });
    }

    [HttpPost("api/v1/devices/shortRaptorAssetIds")]
    [ProducesResponseType(typeof(List<DeviceMatchingModel>), 200)]
    public IActionResult GetMatchingAssets([FromBody] List<long> shortRaptorAssetIds)
    {
      Logger.LogInformation($@"Get MockAssetResolverList for supplied longs: {shortRaptorAssetIds}");

      return Ok(new DeviceMatchingModel { deviceIdentifiers = new List<KeyValuePair<Guid, long>>() });
    }
  }
}
