using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;
using VSS.Common.ServiceDiscovery;
using Xunit;
using System;

namespace CCSS.CWS.Client.UnitTests.Staging
{
  public class DeviceStagingTests : BaseTestClass
  {
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    {
      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddTransient<ICwsAccountClient, CwsAccountClient>();
      services.AddTransient<ICwsDeviceClient, CwsDeviceClient>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();
      services.AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();
      services.AddServiceDiscovery();

      return services;
    }

    protected override bool PretestChecks()
    {
      return CheckTPaaS();
    }

    [Fact(Skip = "manual testing only")]
    public async Task GetDeviceBySerialNumberTest()
    {
      // (new version of GetDevicesBySerialNumber will allow an application token . Setup test with applicationToken
      var serialNumber = "CB1231";

      var expectedAccountId = "MIA";
      var expectedName = "CB1231";
      var expectedType = "CB460";
      var expectedDeviceUid = "08d70d18-2377-863e-00e0-4a00010004cc";
      var expectedSerialNumber = "CB1231";
      var expectedStatus = "Device Not Claimed";

      var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
      var device = await client.GetDeviceBySerialNumber(serialNumber, CustomHeaders());

      // Assert.Equal(expectedAccountId, device.AccountId);
      // Assert.Equal(expectedName, device.DeviceName);
      Assert.Equal(expectedType, device.DeviceType);
      Assert.Equal(expectedDeviceUid, device.Id);
      Assert.Equal(expectedSerialNumber, device.SerialNumber);
      // Assert.Equal(expectedStatus, device.Status);
    }

    [Fact(Skip = "manual testing only")]
    public async Task GetDeviceByDeviceUidTest()
    {
      // (new version of GetDevicesBySerialNumber will allow an application token . Setup test with applicationToken
      var deviceUid = new Guid("08d70d18-2377-863e-00e0-4a00010004cc");

      var expectedAccountId = "MIA";
      var expectedName = "CB1231";
      var expectedType = "CB460";
      var expectedSerialNumber = "CB1231";
      var expectedStatus = "Device Not Claimed";
      var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
      var device = await client.GetDeviceByDeviceUid(deviceUid, CustomHeaders());

      Assert.NotNull(device);
      Assert.Equal(expectedName, device.DeviceName);
      Assert.Equal(expectedType, device.DeviceType);
      Assert.Equal(deviceUid.ToString(), device.Id);
      Assert.Equal(expectedSerialNumber, device.SerialNumber);
    }

    [Fact(Skip = "manual testing only")]
    public async Task GetDevicesForAccountTest()
    {
      // (new version of GetDevicesForAccount will allow an application token . Setup test with applicationToken
      //var accountClient = ServiceProvider.GetRequiredService<ICwsAccountClient>();
      //var accountListResponseModel = await accountClient.GetMyAccounts(TRNHelper.ExtractGuid(userId).Value, CustomHeaders());
      //Assert.NotNull(accountListResponseModel);
      //Assert.True(accountListResponseModel.Accounts.Count > 0);
      var testAccountTRN = "af5a9262-1e29-42df-b199-e2b881a89c88";

      var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
      var deviceList = await client.GetDevicesForAccount(new Guid(testAccountTRN), CustomHeaders());

      Assert.NotNull(deviceList);
      Assert.True(deviceList.Devices.Count >= 0);
    }

  }
}
