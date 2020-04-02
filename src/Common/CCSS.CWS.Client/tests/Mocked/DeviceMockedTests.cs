using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Clients.CWS.Utilities;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;

namespace CCSS.CWS.Client.UnitTests.Mocked
{
  [TestClass]
  public class DeviceMockedTests : BaseTestClass
  {
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    {
      services.AddSingleton(mockWebRequest.Object);
      services.AddSingleton(mockServiceResolution.Object);
      services.AddTransient<ICwsDeviceClient, CwsDeviceClient>();

      return services;
    }

    [TestMethod]
    public void GetDeviceBySerialNumberTest()
    {
      const string serialNumber = "2002J032SW";
      const string expectedDeviceId = "trn::profilex:us-west-2:device:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97"; 
      const string expectedAccountId = "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedDeviceType = "CB460";
      const string expectedDeviceName = "The device Name";
      const string expectedStatus = "ACTIVE";
      const string expectedSerialNumber = serialNumber;

      var deviceResponseModel = new DeviceResponseModel() 
        { Id = expectedDeviceId, AccountId = expectedAccountId, DeviceType = expectedDeviceType, DeviceName = expectedDeviceName, Status = expectedStatus, SerialNumber = expectedSerialNumber};
      
      var route = $"/devices/{serialNumber}";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get device by serial number", mockWebRequest, null, expectedUrl, HttpMethod.Get, deviceResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
        var result = await client.GetDeviceBySerialNumber(serialNumber);

        Assert.IsNotNull(result, "No result from getting device by serialNumber");
        Assert.AreEqual(TRNHelper.ExtractGuidAsString(expectedDeviceId), result.Id);
        Assert.AreEqual(TRNHelper.ExtractGuidAsString(expectedAccountId), result.AccountId);
        Assert.AreEqual(expectedDeviceType, result.DeviceType);
        Assert.AreEqual(expectedDeviceName, result.DeviceName);
        Assert.AreEqual(expectedStatus, result.Status);
        Assert.AreEqual(expectedSerialNumber, result.SerialNumber);
        return true;
      });
    }

    [TestMethod]
    public void GetDeviceByDeviceUidTest()
    {
      const string DeviceId = "trn::profilex:us-west-2:device:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedDeviceId = DeviceId;
      const string expectedAccountId = "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedDeviceType = "CB460";
      const string expectedDeviceName = "The device Name";
      const string expectedStatus = "ACTIVE";
      const string expectedSerialNumber = "2002J032SW";

      var deviceResponseModel = new DeviceResponseModel()
      { Id = expectedDeviceId, AccountId = expectedAccountId, DeviceType = expectedDeviceType, DeviceName = expectedDeviceName, Status = expectedStatus, SerialNumber = expectedSerialNumber };

      var route = $"/devices/{DeviceId}";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get device by serial number", mockWebRequest, null, expectedUrl, HttpMethod.Get, deviceResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
        var result = await client.GetDeviceByDeviceUid(TRNHelper.ExtractGuid(DeviceId).Value);

        Assert.IsNotNull(result, "No result from getting device by serialNumber");
        Assert.AreEqual(TRNHelper.ExtractGuidAsString(expectedDeviceId), result.Id);
        Assert.AreEqual(TRNHelper.ExtractGuidAsString(expectedAccountId), result.AccountId);
        Assert.AreEqual(expectedDeviceType, result.DeviceType);
        Assert.AreEqual(expectedDeviceName, result.DeviceName);
        Assert.AreEqual(expectedStatus, result.Status);
        Assert.AreEqual(expectedSerialNumber, result.SerialNumber);
        return true;
      });
    }

    [TestMethod]
    public void GetDevicesForAccountTest()
    {
      const string accountId = "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedDeviceId = "trn::profilex:us-west-2:device:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedAccountId = accountId;
      const string expectedDeviceType = "CB460";
      const string expectedDeviceName = "The device Name";
      const string expectedStatus = "ACTIVE";
      const string expectedSerialNumber = "2002J032SW";

      var deviceListResponseModel = new DeviceListResponseModel
      {
        HasMore = false,
        Devices = new List<DeviceResponseModel>()
        {
          new DeviceResponseModel() {Id = expectedDeviceId, AccountId = expectedAccountId, DeviceType = expectedDeviceType, DeviceName = expectedDeviceName, Status = expectedStatus, SerialNumber = expectedSerialNumber}
        }
      };
      var route = $"/accounts/{accountId}/devices";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get account devices", mockWebRequest, null, expectedUrl, HttpMethod.Get, deviceListResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
        var result = await client.GetDevicesForAccount(TRNHelper.ExtractGuid(accountId).Value);

        Assert.IsNotNull(result, "No result from getting account devices");
        Assert.IsFalse(result.HasMore);
        Assert.IsNotNull(result.Devices);
        Assert.AreEqual(1, result.Devices.Count);
        Assert.AreEqual(TRNHelper.ExtractGuidAsString(expectedDeviceId), result.Devices[0].Id);
        Assert.AreEqual(TRNHelper.ExtractGuidAsString(expectedAccountId), result.Devices[0].AccountId);
        Assert.AreEqual(expectedDeviceType, result.Devices[0].DeviceType);
        Assert.AreEqual(expectedDeviceName, result.Devices[0].DeviceName);
        Assert.AreEqual(expectedStatus, result.Devices[0].Status);
        Assert.AreEqual(expectedSerialNumber, result.Devices[0].SerialNumber);
        return true;
      });
    }

    [TestMethod]
    public void GetProjectsForDeviceTest()
    {
      const string deviceTrn = "trn::profilex:us-west-2:device:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedAccountTrn = "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedProjectTrn = "trn::profilex:us-west-2:project:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedStatus = "ACTIVE";

      var projectListResponseModel = new ProjectListResponseModel
      {
        HasMore = false,
        Projects = new List<ProjectResponseModel>()
        {
          new ProjectResponseModel() {projectId = expectedProjectTrn, accountId = expectedAccountTrn}
        }
      };
      var route = $"/device/{deviceTrn}/projects";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get projects for a device", mockWebRequest, null, expectedUrl, HttpMethod.Get, projectListResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
        var result = await client.GetProjectsForDevice(TRNHelper.ExtractGuid(deviceTrn).Value);

        Assert.IsNotNull(result, "No result from getting projects for a device");
        Assert.IsFalse(result.HasMore);
        Assert.IsNotNull(result.Projects);
        Assert.AreEqual(1, result.Projects.Count);
        Assert.AreEqual(TRNHelper.ExtractGuidAsString(expectedProjectTrn), result.Projects[0].projectId);
        Assert.AreEqual(TRNHelper.ExtractGuidAsString(expectedAccountTrn), result.Projects[0].accountId);
        return true;
      });
    }

  }
}
