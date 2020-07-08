using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
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
      var deviceLksListResponseModel = new DeviceLKSListResponseModel()
      {
        Devices = new List<DeviceLKSResponseModel>()
        {
          new DeviceLKSResponseModel()
          {
            TRN = TRNHelper.MakeTRN(deviceUid.ToString(), TRNHelper.TRN_DEVICE),
            Latitude = 89.3,
            Longitude = 189.1,
            DeviceType = CWSDeviceTypeEnum.EC520,
            SerialNumber = serialNumber,
            DeviceName = $"{CWSDeviceTypeEnum.EC520}{serialNumber}",
            LastReportedUtc = DateTime.UtcNow.AddDays(-1),
          }
        }
      };

      var route = $"/devicelks";
      var expectedUrl = $"{baseUrl}{route}?projectid={TRNHelper.MakeTRN(projectUid.ToString())}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get devices for project", mockWebRequest, null, expectedUrl, HttpMethod.Get, deviceLksListResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceGatewayClient>();
        var result = await client.GetDevicesLKSForProject(projectUid);

        Assert.NotNull(result);
        Assert.Single(result.Devices);
        Assert.Equal(deviceUid.ToString(), result.Devices[0].Id);
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
      var deviceLksListResponseModel = new DeviceLKSListResponseModel()
      {
        Devices = new List<DeviceLKSResponseModel>()
        {
          new DeviceLKSResponseModel()
          {
            TRN = TRNHelper.MakeTRN(deviceUid.ToString(), TRNHelper.TRN_DEVICE),
            Latitude = 89.3,
            Longitude = 189.1,
            DeviceType = CWSDeviceTypeEnum.EC520,
            SerialNumber = serialNumber,
            DeviceName = $"{CWSDeviceTypeEnum.EC520}{serialNumber}",
            LastReportedUtc = earliestOfInterestUtc.AddDays(1),
          }
        }
      };

      var route = $"/devicelks";
      var expectedUrl = $"{baseUrl}{route}?projectid={TRNHelper.MakeTRN(projectUid.ToString())}&lastReported={earliestOfInterestUtc:yyyy-MM-ddTHH:mm:ssZ}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get devices for project with earliestDate", mockWebRequest, null, expectedUrl, HttpMethod.Get, deviceLksListResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceGatewayClient>();
        var result = await client.GetDevicesLKSForProject(projectUid);

        Assert.NotNull(result);
        Assert.Single(result.Devices);
        Assert.Equal(deviceUid.ToString(), result.Devices[0].Id);
        Assert.Equal(deviceLksListResponseModel.Devices[0].LastReportedUtc, result.Devices[0].LastReportedUtc);
        return true;
      });
    }


    [Fact]
    public void GetDeviceLKS()
    {
      var deviceUid = Guid.NewGuid();
      var deviceType = CWSDeviceTypeEnum.EC520;
      var serialNumber = "12456YU";
      var deviceName = $"{deviceType}-{serialNumber}";

      var deviceLksResponseModel = new DeviceLKSResponseModel()
          {
            TRN = TRNHelper.MakeTRN(deviceUid.ToString(), TRNHelper.TRN_DEVICE),
            Latitude = 89.3,
            Longitude = 189.1,
            DeviceType = deviceType,
            SerialNumber = serialNumber,
            DeviceName = deviceName,
            LastReportedUtc = DateTime.UtcNow.AddDays(1),
           };

      var route = $"/devicelks/{deviceName}";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), 
        route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get device LKS", mockWebRequest, null, expectedUrl, HttpMethod.Get, deviceLksResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceGatewayClient>();
        var result = await client.GetDeviceLKS(deviceName);

        Assert.NotNull(result);
        Assert.Equal(deviceUid.ToString(), result.Id);
        Assert.Equal(deviceName, result.DeviceName);
        Assert.Equal(serialNumber, result.SerialNumber);
        return true;
      });
    }

  }
}
