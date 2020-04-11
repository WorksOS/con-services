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
    { }

    [Route("api/v1/devices/serialnumber/{serialNumber}")]
    [HttpGet]
    public DeviceResponseModel GetDeviceBySerialNumber(string serialNumber)
    {
      var deviceResponseModel = new DeviceResponseModel()
      {
        Id = Guid.NewGuid().ToString(),
        AccountId = Guid.NewGuid().ToString(),
        DeviceType = "EC520",
        DeviceName = "this is a device",
        Status = "Active",
        SerialNumber = serialNumber
      };

      Logger.LogInformation($"{nameof(GetDeviceByDeviceUid)}: serialNumber {serialNumber}. deviceResponseModel {JsonConvert.SerializeObject(deviceResponseModel)}");
      return deviceResponseModel;
    }

    [Route("api/v1/devices/{deviceTrn}")]
    [HttpGet]
    public DeviceResponseModel GetDeviceByDeviceUid(string deviceTrn)
    {
      var deviceResponseModel = new DeviceResponseModel()
      {
        Id = deviceTrn,
        AccountId = Guid.NewGuid().ToString(),
        DeviceType = "EC520",
        DeviceName = "this is a device",
        Status = "Active",
        SerialNumber = "56556565"
      };

      Logger.LogInformation($"{nameof(GetDeviceByDeviceUid)}: deviceTrn {deviceTrn}. deviceResponseModel {JsonConvert.SerializeObject(deviceResponseModel)}");
      return deviceResponseModel;
    }

    // todoMaverick will this go to account service?
    [Route("api/v1/accounts/{accountTrn}/devices")]
    [HttpGet]
    public DeviceListResponseModel GetDevicesForAccount(string accountTrn)
    {
      var deviceListResponseModel = new DeviceListResponseModel()
      {
        Devices = new List<DeviceResponseModel>()
        {
          new DeviceResponseModel()
          {
          Id = Guid.NewGuid().ToString(),
          AccountId = accountTrn,
          DeviceType = "EC520",
          DeviceName = "this is a device",
          Status = "Active",
         SerialNumber = "56556565"
          }
          }
      };

      Logger.LogInformation($"{nameof(GetDevicesForAccount)}: accountTrn {accountTrn}. deviceListResponseModel {JsonConvert.SerializeObject(deviceListResponseModel)}");
      return deviceListResponseModel;
    }


    // todoMaverick will this go to project service?
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
            boundary = new ProjectBoundary()
            {
              type = "Polygon",
              coordinates = new List<double[,]>() { { new double[2, 2] { { 180, 90 }, { 180, 90 } } } }
            }
          }
        }
      };

      Logger.LogInformation($"{nameof(GetProjectsForDevice)}: deviceTrn {deviceTrn}. projectListResponseModel {JsonConvert.SerializeObject(projectListResponseModel)}");
      return projectListResponseModel;
    }
  }
}
