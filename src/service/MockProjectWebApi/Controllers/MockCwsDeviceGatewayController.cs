using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Utils;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus;

namespace MockProjectWebApi.Controllers
{
  public class MockCwsDeviceGatewayController : BaseController
  {

    public MockCwsDeviceGatewayController(ILoggerFactory loggerFactory)
    : base(loggerFactory)
    { }

    [HttpGet]
    [Route("api/v2/devicegateway/devicelks")]
    public List<DeviceLKSResponseModel> GetDevicesLKSForProject(
      [FromQuery] string projectId,
      [FromQuery] DateTime? lastReported)
    {
      Logger.LogInformation($"{nameof(GetDevicesLKSForProject)} projectId {projectId} lastReported {lastReported}");

      var result = new List<DeviceLKSResponseModel>();
      if (TRNHelper.ExtractGuid(projectId) == new Guid(ConstantsUtil.DIMENSIONS_PROJECT_UID))
        result.Add(new DeviceLKSResponseModel()
        {
          TRN = TRNHelper.MakeTRN(ConstantsUtil.DIMENSIONS_SERIAL_DEVICEUID, TRNHelper.TRN_DEVICE),
          assetSerialNumber = ConstantsUtil.DIMENSIONS_SERIAL,
          lat = 89.9,
          lon = 34.6,
          assetType = CWSDeviceTypeEnum.EC520,
          deviceName = $"{CWSDeviceTypeEnum.EC520}-{ConstantsUtil.DIMENSIONS_SERIAL}",
          projectName = "DimensionsProject",
          lastReported = DateTime.UtcNow.AddDays(-1)
        });

      return result;
    }

    [HttpGet]
    [Route("api/v2/devicegateway/devicelks/{deviceName}")]
    public DeviceLKSResponseModel GetDeviceWithLKS(string deviceName)
    {
      Logger.LogInformation($"{nameof(GetDeviceWithLKS)} deviceName {deviceName}");

      if (deviceName == $"{CWSDeviceTypeEnum.EC520}-{ConstantsUtil.DIMENSIONS_SERIAL}")
        return new DeviceLKSResponseModel()
        {
          TRN = TRNHelper.MakeTRN(ConstantsUtil.DIMENSIONS_SERIAL_DEVICEUID, TRNHelper.TRN_DEVICE),
          assetSerialNumber = ConstantsUtil.DIMENSIONS_SERIAL,
          lat = 89.9,
          lon = 34.6,
          assetType = CWSDeviceTypeEnum.EC520,
          deviceName = $"{CWSDeviceTypeEnum.EC520}-{ConstantsUtil.DIMENSIONS_SERIAL}",
          projectName = "DimensionsProject",
          lastReported = DateTime.UtcNow.AddDays(-1)
        };

      return null;
    }
  }
}
