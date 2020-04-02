using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;
using VSS.Common.ServiceDiscovery;
using VSS.Common.Abstractions.Clients.CWS.Utilities;

namespace CCSS.CWS.Client.UnitTests.Staging
{
  [TestClass]
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

    [TestMethod]
    [Ignore] // todoMaverick does this need a user token Sankari? 
             // (requires a user token. This is ok as will have one via from ProjectSvc) 
    public async Task GetDevicesForAccountTest()
    {
      var accountClient = ServiceProvider.GetRequiredService<ICwsAccountClient>();
      var accountListResponseModel = await accountClient.GetMyAccounts(TRNHelper.ExtractGuid(userId).Value, CustomHeaders());
      Assert.IsNotNull(accountListResponseModel, "No result from getting my accounts");
      Assert.IsTrue(accountListResponseModel.Accounts.Count > 0);

      var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
      var deviceList = await client.GetDevicesForAccount(TRNHelper.ExtractGuid(accountListResponseModel.Accounts[0].Id).Value, CustomHeaders());

      Assert.IsNotNull(deviceList, "No result from getting device list");
      Assert.IsTrue(deviceList.Devices.Count > 0);
    }
       
  }
}
