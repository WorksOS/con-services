using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockProjectDeviceInternalController : BaseController
  {

    public MockProjectDeviceInternalController(ILoggerFactory loggerFactory)
    : base(loggerFactory)
    {
    }

    [HttpGet]
    [Route("internal/v1/device/serialnumber")] 
    [Route("api/v1/device/applicationcontext/serialnumber")] // todoJeannie obsolete once ProjectSvc changes merged to master
    // called internally by TFA only
    public DeviceDataSingleResult GetDeviceBySerialNumber([FromQuery] string serialNumber)
    {
      Logger.LogInformation($"{nameof(GetDeviceBySerialNumber)} serialNumber {serialNumber}");
      return new DeviceDataSingleResult();
    }

    [HttpGet]
    [Route("internal/v1/device/shortRaptorAssetId")]
    [Route("api/v1/device/applicationcontext/shortRaptorAssetId")] // todoJeannie obsolete once ProjectSvc changes merged to master
    // called internally by TFA only
    public DeviceDataSingleResult GetDevice([FromQuery] int shortRaptorAssetId)
    {
      Logger.LogInformation($"{nameof(GetDevice)} serialNumber {shortRaptorAssetId}");
      return new DeviceDataSingleResult();
    }

    [HttpGet]
    [Route("internal/v1/device/{deviceUid}/projects")]
    [Route("api/v1/device/applicationcontext/{deviceUid}/projects")] // todoJeannie obsolete once ProjectSvc changes merged to master
    // called internally by TFA only
    public ProjectDataResult GetProjectsForDevice(string deviceUid)
    {
      Logger.LogInformation($"{nameof(GetProjectsForDevice)} deviceUid {deviceUid}");
      return new ProjectDataResult();
    }

    [HttpGet]
    [Route("internal/v1/device/{deviceUid}/accounts")]
    // called internally by TFA only
    public DeviceCustomerListDataResult GetAccountsForDevice(string deviceUid)
    {
      Logger.LogInformation($"{nameof(GetAccountsForDevice)} deviceUid {deviceUid}");
      return new DeviceCustomerListDataResult();
    }

    [HttpGet]
    [Route("internal/v1/device/{deviceUid}/account")]
    // called internally by TFA only
    public DeviceCustomerSingleDataResult GetAccountForDevice(string deviceUid)
    {
      Logger.LogInformation($"{nameof(GetAccountsForDevice)} deviceUid {deviceUid}");
      return new DeviceCustomerSingleDataResult();
    }
  }
}
