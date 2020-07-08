using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IntegrationTests.UtilityClasses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TestUtility;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace IntegrationTests.WebApiTests
{
  public class DeviceTests : WebApiTestsBase
  {
    public const string DIMENSIONS_PROJECT_UID = "ff91dd40-1569-4765-a2bc-014321f76ace";
    public const string DIMENSIONS_CUSTOMER_UID = "87bdf851-44c5-e311-aa77-00505688274d";

    public const string DIMENSIONS_SERIAL = "5051593854";
    public const string DIMENSIONS_SERIAL_DEVICEUID = "039c1ee8-1f21-e311-9ee2-00505688274d";


    [Fact]
    public async Task GetDevicesLKSForProject_HappyPath()
    {
      const string testText = "GetDevicesLKS test 1";
      Msg.Title(testText, "For existing project");
      var ts = new TestSupport();

      var response = await ts.GetDeviceLKSList(DIMENSIONS_CUSTOMER_UID, DIMENSIONS_PROJECT_UID);
      Assert.NotNull(response); 
      var deviceStatusDescriptorsListResult = JsonConvert.DeserializeObject<DeviceStatusDescriptorsListResult>(response);
      
      Assert.NotNull(deviceStatusDescriptorsListResult);
      var deviceList = deviceStatusDescriptorsListResult.DeviceStatusDescriptors.ToList();
      Assert.Single(deviceList);
      Assert.Equal(DIMENSIONS_SERIAL_DEVICEUID, deviceList[0].DeviceUid);
      Assert.Equal(DIMENSIONS_SERIAL, deviceList[0].SerialNumber);
      Assert.Equal(89.9, deviceList[0].Latitude);
      Assert.Equal(34.6, deviceList[0].Longitude);
      Assert.Equal(CWSDeviceTypeEnum.EC520, deviceList[0].DeviceType);
      Assert.Equal($"{CWSDeviceTypeEnum.EC520}-{DIMENSIONS_SERIAL}", deviceList[0].DeviceName);
      Assert.Equal($"DimensionsProject", deviceList[0].ProjectName);
      Assert.NotNull(deviceList[0].LastReportedUtc);
      Assert.True(deviceList[0].LastReportedUtc.Value > DateTime.UtcNow.AddDays(-30));
    }

    [Fact]
    public async Task GetDevicesLKSForProject_NoDevices()
    {
      const string testText = "GetDevicesLKS test 2";
      Msg.Title(testText, "No devices");
      var ts = new TestSupport();

      var response = await ts.GetDeviceLKSList(DIMENSIONS_CUSTOMER_UID, Guid.NewGuid().ToString());
      Assert.NotNull(response);
      var deviceStatusDescriptorsListResult = JsonConvert.DeserializeObject<DeviceStatusDescriptorsListResult>(response);

      Assert.NotNull(deviceStatusDescriptorsListResult);
      var deviceList = deviceStatusDescriptorsListResult.DeviceStatusDescriptors.ToList();
      Assert.Empty(deviceList);
    }


    [Fact]
    public async Task GetDeviceLKS_HappyPath()
    {
      const string testText = "GetDevicesLKS test 3";
      Msg.Title(testText, "For existing device");
      var ts = new TestSupport();

      var deviceName = $"{CWSDeviceTypeEnum.EC520}-{DIMENSIONS_SERIAL}";
      var response = await ts.GetDeviceLKS(DIMENSIONS_CUSTOMER_UID, deviceName);
      Assert.NotNull(response);
      var deviceStatusDescriptorSingleResult = JsonConvert.DeserializeObject<DeviceStatusDescriptorSingleResult>(response);

      Assert.NotNull(deviceStatusDescriptorSingleResult);
      Assert.Equal(DIMENSIONS_SERIAL_DEVICEUID, deviceStatusDescriptorSingleResult.DeviceDescriptor.DeviceUid);
      Assert.Equal(DIMENSIONS_SERIAL, deviceStatusDescriptorSingleResult.DeviceDescriptor.SerialNumber);
      Assert.Equal(89.9, deviceStatusDescriptorSingleResult.DeviceDescriptor.Latitude);
      Assert.Equal(34.6, deviceStatusDescriptorSingleResult.DeviceDescriptor.Longitude);
      Assert.Equal(CWSDeviceTypeEnum.EC520, deviceStatusDescriptorSingleResult.DeviceDescriptor.DeviceType);
      Assert.Equal(deviceName, deviceStatusDescriptorSingleResult.DeviceDescriptor.DeviceName);
      Assert.Equal($"DimensionsProject", deviceStatusDescriptorSingleResult.DeviceDescriptor.ProjectName);
      Assert.NotNull(deviceStatusDescriptorSingleResult.DeviceDescriptor.LastReportedUtc);
      Assert.True(deviceStatusDescriptorSingleResult.DeviceDescriptor.LastReportedUtc.Value > DateTime.UtcNow.AddDays(-30));
    }

    [Fact]
    public async Task GetDeviceLKS_NoDevice()
    {
      const string testText = "GetDevicesLKS test 3";
      Msg.Title(testText, "For existing device");
      var ts = new TestSupport();

      var deviceName = $"{CWSDeviceTypeEnum.EC520}-woteva";
      var result =  await ts.GetDeviceLKS(DIMENSIONS_CUSTOMER_UID, deviceName, HttpStatusCode.NotFound);
      Assert.Empty(result);
    }
  }
}
