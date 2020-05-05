using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using Xunit;

namespace CCSS.CWS.Client.UnitTests.Mocked
{
  public class DeviceMockedTests : BaseTestClass
  {
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    {
      services.AddSingleton(mockWebRequest.Object);
      services.AddSingleton(mockServiceResolution.Object);
      services.AddTransient<ICwsDeviceClient, CwsDeviceClient>();

      return services;
    }

    [Fact]
    public void GetDeviceBySerialNumberTest()
    {
      const string serialNumber = "2002J032SW";
      const string expectedDeviceId = "trn::profilex:us-west-2:device:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97"; 
      const string expectedDeviceType = "CB460";
      const string expectedDeviceName = "The device Name";
      const string expectedSerialNumber = serialNumber;

      var deviceResponseModel = new DeviceResponseModel() 
        { Id = expectedDeviceId, DeviceType = expectedDeviceType, DeviceName = expectedDeviceName, SerialNumber = expectedSerialNumber};
      
      var route = $"/devices/getDeviceWithSerialNumber";
      var queryParameters = new List<KeyValuePair<string, string>>{
          new KeyValuePair<string, string>("serialNumber", serialNumber)};
      var expectedUrl = $"{baseUrl}{route}?serialNumber={serialNumber}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, queryParameters)).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get device by serial number", mockWebRequest, null, expectedUrl, HttpMethod.Get, deviceResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
        var result = await client.GetDeviceBySerialNumber(serialNumber);

        Assert.NotNull(result);
        Assert.Equal(TRNHelper.ExtractGuidAsString(expectedDeviceId), result.Id);
        Assert.Equal(expectedDeviceType, result.DeviceType);
        Assert.Equal(expectedDeviceName, result.DeviceName);
        Assert.Equal(expectedSerialNumber, result.SerialNumber);
        return true;
      });
    }

    [Fact]
    public void GetDeviceByDeviceUidTest()
    {
      const string DeviceId = "trn::profilex:us-west-2:device:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedDeviceId = DeviceId;
      const string expectedDeviceType = "CB460";
      const string expectedDeviceName = "The device Name";
      const string expectedSerialNumber = "2002J032SW";

      var deviceResponseModel = new DeviceResponseModel()
      { Id = expectedDeviceId, DeviceType = expectedDeviceType, DeviceName = expectedDeviceName, SerialNumber = expectedSerialNumber };

      var route = $"/devices/{DeviceId}";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get device by deviceUid", mockWebRequest, null, expectedUrl, HttpMethod.Get, deviceResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
        var result = await client.GetDeviceByDeviceUid(TRNHelper.ExtractGuid(DeviceId).Value);

        Assert.NotNull(result);
        Assert.Equal(TRNHelper.ExtractGuidAsString(expectedDeviceId), result.Id);
        Assert.Equal(expectedDeviceType, result.DeviceType);
        Assert.Equal(expectedDeviceName, result.DeviceName);
        Assert.Equal(expectedSerialNumber, result.SerialNumber);
        return true;
      });
    }

    [Fact]
    public void GetDevicesForAccountTest()
    {
      const string accountId = "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedDeviceId = "trn::profilex:us-west-2:device:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedDeviceType = "CB460";
      const string expectedDeviceName = "The device Name";
      const string expectedSerialNumber = "2002J032SW";
      const RelationStatusEnum relationStatus = RelationStatusEnum.Active;
      const TCCDeviceStatusEnum tccDeviceStatus = TCCDeviceStatusEnum.Pending;

      var deviceListResponseModel = new DeviceListResponseModel
      {
        HasMore = false,
        Devices = new List<DeviceFromListResponseModel>()
        {
          new DeviceFromListResponseModel() {Id = expectedDeviceId, DeviceType = expectedDeviceType, DeviceName = expectedDeviceName, SerialNumber = expectedSerialNumber, RelationStatus = relationStatus, TccDeviceStatus = tccDeviceStatus}
        }
      };
      var route = $"/accounts/{accountId}/devices";
      var expectedUrl = $"{baseUrl}{route}?from=0&limit=20&includeTccRegistrationStatus=true";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get account devices", mockWebRequest, null, expectedUrl, HttpMethod.Get, deviceListResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
        var result = await client.GetDevicesForAccount(TRNHelper.ExtractGuid(accountId).Value);

        Assert.NotNull(result);
        Assert.False(result.HasMore);
        Assert.NotNull(result.Devices);
        Assert.Single(result.Devices);
        Assert.Equal(TRNHelper.ExtractGuidAsString(expectedDeviceId), result.Devices[0].Id);
        Assert.Equal(expectedDeviceType, result.Devices[0].DeviceType);
        Assert.Equal(expectedDeviceName, result.Devices[0].DeviceName);
        Assert.Equal(expectedSerialNumber, result.Devices[0].SerialNumber);
        return true;
      });
    }

    [Fact]
    public void GetProjectsForDeviceTest()
    {
      const string deviceTrn = "trn::profilex:us-west-2:device:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedAccountTrn = "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedProjectTrn = "trn::profilex:us-west-2:project:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";

      var projectListResponseModel = new ProjectListResponseModel
      {
        HasMore = false,
        Projects = new List<ProjectResponseModel>()
        {
          new ProjectResponseModel() {projectId = expectedProjectTrn, accountId = expectedAccountTrn}
        }
      };
      var route = $"/device/{deviceTrn}/projects";
      var expectedUrl = $"{baseUrl}{route}?from=0&limit=20";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get projects for a device", mockWebRequest, null, expectedUrl, HttpMethod.Get, projectListResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
        var result = await client.GetProjectsForDevice(TRNHelper.ExtractGuid(deviceTrn).Value);

        Assert.NotNull(result);
        Assert.False(result.HasMore);
        Assert.NotNull(result.Projects);
        Assert.Single(result.Projects);
        Assert.Equal(TRNHelper.ExtractGuidAsString(expectedProjectTrn), result.Projects[0].projectId);
        Assert.Equal(TRNHelper.ExtractGuidAsString(expectedAccountTrn), result.Projects[0].accountId);
        return true;
      });
    }


    [Fact]
    public void GetAccountsForDeviceTest()
    {
      const string deviceTrn = "trn::profilex:us-west-2:device:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedAccountTrn = "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";

      var deviceAccountListResponseModel = new DeviceAccountListResponseModel
      {
        HasMore = false,
        Accounts = new List<DeviceAccountResponseModel>()
        {
          new DeviceAccountResponseModel() 
          {
            Id = expectedAccountTrn, 
            AccountName = "an account name",
            RelationStatus = RelationStatusEnum.Active,
            TccDeviceStatus = TCCDeviceStatusEnum.Pending
          }
        }
      };
      var route = $"/devices/{deviceTrn}/accounts";
      var expectedUrl = $"{baseUrl}{route}?from=0&limit=20";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get accounts for a device", mockWebRequest, null, expectedUrl, HttpMethod.Get, deviceAccountListResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
        var result = await client.GetAccountsForDevice(TRNHelper.ExtractGuid(deviceTrn).Value);

        Assert.NotNull(result);
        Assert.False(result.HasMore);
        Assert.NotNull(result.Accounts);
        Assert.Single(result.Accounts);
        Assert.Equal(TRNHelper.ExtractGuidAsString(expectedAccountTrn), result.Accounts[0].Id);
        Assert.Equal(RelationStatusEnum.Active, result.Accounts[0].RelationStatus);
        return true;
      });
    }

  }
}
