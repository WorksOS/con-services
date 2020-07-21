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
          DeviceTrn = TRNHelper.MakeTRN(ConstantsUtil.DIMENSIONS_SERIAL_DEVICEUID, TRNHelper.TRN_DEVICE),
          AssetSerialNumber = ConstantsUtil.DIMENSIONS_SERIAL,
          Latitude = 89.9, Longitude = 34.6,
          AssetType = "Grader",
          DeviceName = $"{CWSDeviceTypeEnum.EC520}-{ConstantsUtil.DIMENSIONS_SERIAL}",
          ProjectName = "DimensionsProject",
          LastReportedUtc = DateTime.UtcNow.AddDays(-1)
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
          DeviceTrn = TRNHelper.MakeTRN(ConstantsUtil.DIMENSIONS_SERIAL_DEVICEUID, TRNHelper.TRN_DEVICE),
          AssetSerialNumber = ConstantsUtil.DIMENSIONS_SERIAL,
          Latitude = 89.9, Longitude = 34.6,
          AssetType = "Excavator",
          DeviceName = $"{CWSDeviceTypeEnum.EC520}-{ConstantsUtil.DIMENSIONS_SERIAL}",
          ProjectName = "DimensionsProject",
          LastReportedUtc = DateTime.UtcNow.AddDays(-1)
        };

      return null;
    }
  }
}
