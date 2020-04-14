using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockProjectDeviceController : BaseController
  {

    public MockProjectDeviceController(ILoggerFactory loggerFactory)
    : base(loggerFactory)
    {
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
