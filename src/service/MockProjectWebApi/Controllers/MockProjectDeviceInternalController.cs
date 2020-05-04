using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Utils;
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
    public DeviceDataSingleResult GetDeviceBySerialNumber([FromQuery] string serialNumber)
    {
      Logger.LogInformation($"{nameof(GetDeviceBySerialNumber)} serialNumber {serialNumber}");
      if (serialNumber == ConstantsUtil.DIMENSIONS_SERIAL)
        return new DeviceDataSingleResult(new DeviceData() {CustomerUID = ConstantsUtil.DIMENSIONS_CUSTOMER_UID, DeviceUID = ConstantsUtil.DIMENSIONS_SERIAL_DEVICEUID, SerialNumber = ConstantsUtil.DIMENSIONS_SERIAL, ShortRaptorAssetId = ConstantsUtil.DIMENSIONS_SERIAL_ASSETID });

      return new DeviceDataSingleResult(code: 100, message: "Unable to locate device by serialNumber in cws");
    }

    [HttpGet]
    [Route("internal/v1/device/shortRaptorAssetId")]
    [Route("api/v1/device/applicationcontext/shortRaptorAssetId")] // todoJeannie obsolete once ProjectSvc changes merged to master
    public DeviceDataSingleResult GetDevice([FromQuery] int shortRaptorAssetId)
    {
      Logger.LogInformation($"{nameof(GetDevice)} serialNumber {shortRaptorAssetId}");
      if (shortRaptorAssetId == ConstantsUtil.DIMENSIONS_PROJECT_ID)
        return new DeviceDataSingleResult(new DeviceData() { CustomerUID = ConstantsUtil.DIMENSIONS_CUSTOMER_UID, DeviceUID = ConstantsUtil.DIMENSIONS_SERIAL_DEVICEUID, SerialNumber = ConstantsUtil.DIMENSIONS_SERIAL, ShortRaptorAssetId = ConstantsUtil.DIMENSIONS_SERIAL_ASSETID });

      return new DeviceDataSingleResult(code: 100, message: "Unable to locate device by serialNumber in cws");
    }

    [HttpGet]
    [Route("internal/v1/device/{deviceUid}/projects")]
    [Route("api/v1/device/applicationcontext/{deviceUid}/projects")] // todoJeannie obsolete once ProjectSvc changes merged to master
    public ProjectDataListResult GetProjectsForDevice(string deviceUid)
    {
      Logger.LogInformation($"{nameof(GetProjectsForDevice)} deviceUid {deviceUid}");

      if (deviceUid == ConstantsUtil.DIMENSIONS_SERIAL_DEVICEUID)
        return new ProjectDataListResult() 
          {ProjectDescriptors = new List<ProjectData>() 
            {new ProjectData() {CustomerUID = ConstantsUtil.DIMENSIONS_CUSTOMER_UID, ProjectUID = ConstantsUtil.DIMENSIONS_PROJECT_UID, ShortRaptorProjectId = ConstantsUtil.DIMENSIONS_PROJECT_ID, IsArchived = false}}};

      return new ProjectDataListResult(code: 105, message: "Unable to locate projects for device in cws");
    }

    // todoMaverick
    //[HttpGet]
    //[Route("internal/v1/device/{deviceUid}/accounts")]
    //// called internally by TFA only
    //public DeviceCustomerListDataResult GetAccountsForDevice(string deviceUid)
    //{
    //  Logger.LogInformation($"{nameof(GetAccountsForDevice)} deviceUid {deviceUid}");
    //  return new DeviceCustomerListDataResult();
    //}

    //[HttpGet]
    //[Route("internal/v1/device/{deviceUid}/account")]
    //// called internally by TFA only
    //public DeviceCustomerSingleDataResult GetAccountForDevice(string deviceUid)
    //{
    //  Logger.LogInformation($"{nameof(GetAccountsForDevice)} deviceUid {deviceUid}");
    //  return new DeviceCustomerSingleDataResult();
    //}
  }
}
