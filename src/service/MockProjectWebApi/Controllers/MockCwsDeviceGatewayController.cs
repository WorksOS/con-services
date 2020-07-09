using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Utils;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockCwsDeviceGatewayController : BaseController
  {

    public MockCwsDeviceGatewayController(ILoggerFactory loggerFactory)
    : base(loggerFactory)
    { }

    [HttpGet]
    [Route("api/v2/devicelks")]
    public DeviceLKSListResponseModel GetDevicesLKSForProject(
      [FromQuery] string projectid,
      [FromQuery] DateTime? lastReported)
    {
      Logger.LogInformation($"{nameof(GetDevicesLKSForProject)} projectid {projectid} lastReported {lastReported}");

      var result = new DeviceLKSListResponseModel();
      if (TRNHelper.ExtractGuid(projectid) == new Guid(ConstantsUtil.DIMENSIONS_PROJECT_UID))
        result.Devices = new List<DeviceLKSResponseModel>()
        { new DeviceLKSResponseModel() 
          { TRN = TRNHelper.MakeTRN(ConstantsUtil.DIMENSIONS_SERIAL_DEVICEUID, TRNHelper.TRN_DEVICE),
            SerialNumber = ConstantsUtil.DIMENSIONS_SERIAL,
            Latitude = 89.9, Longitude = 34.6,
            DeviceType = CWSDeviceTypeEnum.EC520,
            DeviceName = $"{CWSDeviceTypeEnum.EC520}-{ConstantsUtil.DIMENSIONS_SERIAL}",
            ProjectName = "DimensionsProject",
            LastReportedUtc = DateTime.UtcNow.AddDays(-1)
          }};

      return result;
    }

    [HttpGet]
    [Route("api/v2/devicelks/{deviceName}")]
    public DeviceLKSResponseModel GetDeviceWithLKS(string deviceName )
    {
      Logger.LogInformation($"{nameof(GetDeviceWithLKS)} deviceName {deviceName}");

      if (deviceName == $"{CWSDeviceTypeEnum.EC520}-{ConstantsUtil.DIMENSIONS_SERIAL}")
        return new DeviceLKSResponseModel()
        {
          TRN = TRNHelper.MakeTRN(ConstantsUtil.DIMENSIONS_SERIAL_DEVICEUID, TRNHelper.TRN_DEVICE),
          SerialNumber = ConstantsUtil.DIMENSIONS_SERIAL,
          Latitude = 89.9,
          Longitude = 34.6,
          DeviceType = CWSDeviceTypeEnum.EC520,
          DeviceName = $"{CWSDeviceTypeEnum.EC520}-{ConstantsUtil.DIMENSIONS_SERIAL}",
          ProjectName = "DimensionsProject",
          LastReportedUtc = DateTime.UtcNow.AddDays(-1)
        };

      return null;
    }
  }
}
