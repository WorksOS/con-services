using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
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
    public void Test_GetAccountDevices()
    {
      const string accountId = "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedId = "trn::profilex:us-west-2:device:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedDeviceType = "CB460";
      const string expectedSerialNumber = "2002J032SW";

      var deviceListModel = new DeviceListResponseModel
      {
        HasMore = false,
        Devices = new List<DeviceResponseModel>()
        {
          new DeviceResponseModel() {Id = expectedId, DeviceType = expectedDeviceType, SerialNumber = expectedSerialNumber}
        }
      };
      var route = $"/accounts/{accountId}/devices";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get account devices", mockWebRequest, null, expectedUrl, HttpMethod.Get, deviceListModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
        var result = await client.GetDevicesForAccount(accountId);

        Assert.IsNotNull(result, "No result from getting account devices");
        Assert.IsFalse(result.HasMore);
        Assert.IsNotNull(result.Devices);
        Assert.AreEqual(1, result.Devices.Count);
        Assert.AreEqual(result.Devices[0].Id, expectedId);
        Assert.AreEqual(result.Devices[0].DeviceType, expectedDeviceType);
        Assert.AreEqual(result.Devices[0].SerialNumber, expectedSerialNumber);
        return true;
      });
    }
  }
}
