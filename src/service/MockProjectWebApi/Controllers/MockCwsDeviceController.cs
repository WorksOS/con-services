using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockCwsDeviceController : BaseController
  {
    public MockCwsDeviceController(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }

    [Route("api/v1/devices/serialnumber")]
    [HttpGet]
    public DeviceResponseModel GetDeviceBySerialNumber([FromQuery] string serialNumber)
    {
      var deviceResponseModel = new DeviceResponseModel() {Id = Guid.NewGuid().ToString(), DeviceType = "EC520", DeviceName = "this is a device", SerialNumber = serialNumber};

      Logger.LogInformation($"{nameof(GetDeviceByDeviceUid)}: serialNumber {serialNumber}. deviceResponseModel {JsonConvert.SerializeObject(deviceResponseModel)}");
      return deviceResponseModel;
    }

    [Route("api/v1/devices/{deviceTrn}")]
    [HttpGet]
    public DeviceResponseModel GetDeviceByDeviceUid(string deviceTrn)
    {
      var deviceResponseModel = new DeviceResponseModel() {Id = deviceTrn, DeviceType = "EC520", DeviceName = "this is a device", SerialNumber = "56556565"};

      Logger.LogInformation($"{nameof(GetDeviceByDeviceUid)}: deviceTrn {deviceTrn}. deviceResponseModel {JsonConvert.SerializeObject(deviceResponseModel)}");
      return deviceResponseModel;
    }

    [Route("api/v1/accounts/{accountTrn}/devices")]
    [HttpGet]
    public DeviceListResponseModel GetDevicesForAccount(string accountTrn)
    {
      var deviceListResponseModel = new DeviceListResponseModel() {Devices = new List<DeviceFromListResponseModel>() {new DeviceFromListResponseModel() {Id = Guid.NewGuid().ToString(), DeviceType = "EC520", DeviceName = "this is a device", SerialNumber = "56556565"}}};

      Logger.LogInformation($"{nameof(GetDevicesForAccount)}: accountTrn {accountTrn}. deviceListResponseModel {JsonConvert.SerializeObject(deviceListResponseModel)}");
      return deviceListResponseModel;
    }


    [Route("api/v1/device/{deviceTrn}/projects")]
    [HttpGet]
    public ProjectListResponseModel GetProjectsForDevice(string deviceTrn)
    {
      var projectListResponseModel = new ProjectListResponseModel()
      {
        Projects = new List<ProjectResponseModel>()
        {
          new ProjectResponseModel()
          {
            accountId = Guid.NewGuid().ToString(),
            projectId = Guid.NewGuid().ToString(),
            projectName = "this is a project",
            timezone = "Timbucktoo",
            boundary = new ProjectBoundary() {type = "Polygon", coordinates = new List<double[,]>() {{new double[2, 2] {{180, 90}, {180, 90}}}}}
          }
        }
      };

      Logger.LogInformation($"{nameof(GetProjectsForDevice)}: deviceTrn {deviceTrn}. projectListResponseModel {JsonConvert.SerializeObject(projectListResponseModel)}");
      return projectListResponseModel;
    }
  }
}
