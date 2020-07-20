using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using Xunit;

namespace CCSS.CWS.Client.UnitTests.Mocked
{
  public class DeviceGatewayMockedTests : BaseTestClass
  {
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    {
      services.AddSingleton(mockWebRequest.Object);
      services.AddSingleton(mockServiceResolution.Object);
      services.AddTransient<ICwsDeviceGatewayClient, CwsDeviceGatewayClient>();

      return services;
    }

    [Fact]
    public void GetDevicesLKSForProject()
    {
      var deviceUid = Guid.NewGuid();
      var projectUid = Guid.NewGuid();

      var serialNumber = "12456YU";
      var devices = new List<DeviceLKSResponseModel>()
        {
          new DeviceLKSResponseModel()
          {
            DeviceTrn = TRNHelper.MakeTRN(deviceUid.ToString(), TRNHelper.TRN_DEVICE),
            Latitude = 89.3, Longitude = 189.1,
            AssetType = "Grader",
            AssetSerialNumber = serialNumber,
            DeviceName = $"{CWSDeviceTypeEnum.EC520}{serialNumber}",
            LastReportedUtc = DateTime.UtcNow.AddDays(-1),
          }
      };

      var route = $"/devicegateway/devicelks";
      var expectedUrl = $"{baseUrl}{route}?projectid={TRNHelper.MakeTRN(projectUid.ToString())}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get devices for project", mockWebRequest, null, expectedUrl, HttpMethod.Get, devices, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceGatewayClient>();
        var result = await client.GetDevicesLKSForProject(projectUid);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(deviceUid.ToString(), result[0].DeviceUid);
        return true;
      });
    }

    [Fact]
    public void GetDevicesLKSForProject_earliestReportDate()
    {
      var deviceUid = Guid.NewGuid();
      var projectUid = Guid.NewGuid();
      var earliestOfInterestUtc = DateTime.UtcNow.AddDays(-2).AddHours(4.5);

      var serialNumber = "12456YU";
      var devices = new List<DeviceLKSResponseModel>()
        {
          new DeviceLKSResponseModel()
          {
            DeviceTrn = TRNHelper.MakeTRN(deviceUid.ToString(), TRNHelper.TRN_DEVICE),
            Latitude = 89.3, Longitude = 189.1,
            AssetType = "Excavator",
            AssetSerialNumber = serialNumber,
            DeviceName = $"{CWSDeviceTypeEnum.EC520}{serialNumber}",
            LastReportedUtc = earliestOfInterestUtc.AddDays(1),
          }
      };

      var route = $"/devicegateway/devicelks";
      var expectedUrl = $"{baseUrl}{route}?projectid={TRNHelper.MakeTRN(projectUid.ToString())}&lastReported={earliestOfInterestUtc:yyyy-MM-ddTHH:mm:ssZ}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get devices for project with earliestDate", mockWebRequest, null, expectedUrl, HttpMethod.Get, devices, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceGatewayClient>();
        var result = await client.GetDevicesLKSForProject(projectUid);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(deviceUid.ToString(), result[0].DeviceUid);
        Assert.Equal(devices[0].LastReportedUtc, result[0].LastReportedUtc);
        return true;
      });
    }

    [Fact]
    public void GetDeviceLKS()
    {
      var deviceUid = Guid.NewGuid();
      var deviceType = CWSDeviceTypeEnum.CB450;
      var serialNumber = "12456YU";
      var deviceName = $"{deviceType}-{serialNumber}";

      var deviceLksResponseModel = new DeviceLKSResponseModel()
      {
        DeviceTrn = TRNHelper.MakeTRN(deviceUid.ToString(), TRNHelper.TRN_DEVICE),
        Latitude = 89.3, Longitude = 189.1,
        AssetType = "BargeMountedExcavator",
        AssetSerialNumber = serialNumber,
        DeviceName = deviceName,
        LastReportedUtc = DateTime.UtcNow.AddDays(1),
      };

      var route = $"/devicegateway/devicelks/{deviceName}";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(),
        route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get device LKS", mockWebRequest, null, expectedUrl, HttpMethod.Get, deviceLksResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceGatewayClient>();
        var result = await client.GetDeviceLKS(deviceName);

        Assert.NotNull(result);
        Assert.Equal(deviceUid.ToString(), result.DeviceUid);
        Assert.Equal(deviceName, result.DeviceName);
        Assert.Equal(serialNumber, result.AssetSerialNumber);
        return true;
      });
    }

    [Fact]
    public void CreateDeviceLKS()
    {
      var deviceType = CWSDeviceTypeEnum.EC520;
      var serialNumber = "12456YU";
      var deviceName = $"{deviceType}-{serialNumber}";

      var deviceLKSModel = new DeviceLKSModel()
      {
        TimeStamp = DateTime.UtcNow.AddDays(1),
        Latitude = -2,
        Longitude = 3,
        Height = 1,
        AssetSerialNumber = serialNumber,
        AssetType = "Dozer",
        AssetNickname = "Little Nicky",
        DesignName = "Highway to hell",
        AppName = "Trimble Groundworks",
        AppVersion = "1.1.19200.96",
        Devices = new List<ConnectedDevice> { new ConnectedDevice { Model = "SNM940", SerialNumber = "123456" } }
      };

      var route = $"/devicegateway/status/{deviceName}";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(),
        route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Post device LKS", mockWebRequest, null, expectedUrl, 
        HttpMethod.Post, async () =>

      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceGatewayClient>();
        await client.CreateDeviceLKS(deviceName, deviceLKSModel);
        return true;
      });
    }

  }
}
