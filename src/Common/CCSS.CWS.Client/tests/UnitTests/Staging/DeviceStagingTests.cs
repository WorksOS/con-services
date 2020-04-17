using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;
using VSS.Common.ServiceDiscovery;
using Xunit;

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

    [Fact(Skip = "waiting on CCSSSCON-136")] 
    public async Task GetDevicesForAccountTest()
    {
      // (new version of GetDevicesForAccount will allow an application token . Setup test with applicationToken
      var accountClient = ServiceProvider.GetRequiredService<ICwsAccountClient>();
      var accountListResponseModel = await accountClient.GetMyAccounts(TRNHelper.ExtractGuid(userId).Value, CustomHeaders());
      Assert.NotNull(accountListResponseModel);
      Assert.True(accountListResponseModel.Accounts.Count > 0);

      var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
      var deviceList = await client.GetDevicesForAccount(TRNHelper.ExtractGuid(accountListResponseModel.Accounts[0].Id).Value, CustomHeaders());

      Assert.NotNull(deviceList);
      Assert.True(deviceList.Devices.Count > 0);
    }
       
  }
}
