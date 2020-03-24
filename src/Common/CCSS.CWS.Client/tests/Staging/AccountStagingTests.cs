using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;
using VSS.Common.ServiceDiscovery;

namespace CCSS.CWS.Client.UnitTests.Staging
{
  [TestClass]
  public class AccountStagingTests : BaseTestClass
  {
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    { 
      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddTransient<ICwsAccountClient, CwsAccountClient>();
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
    public async Task Test_GetMyAccounts()
    {
      var client = ServiceProvider.GetRequiredService<ICwsAccountClient>();
      var result = await client.GetMyAccounts(userId, CustomHeaders());

      Assert.IsNotNull(result, "No result from getting my accounts");
      Assert.IsNotNull(result.Accounts);
      Assert.IsTrue(result.Accounts.Count > 0);
    }

    [TestMethod]
    [Ignore]
    public async Task Test_GetAccountsForUser()
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    [Ignore]
    public async Task Test_GetAccountForUser()
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    public async Task Test_GetDeviceAccounts()
    {
      var client = ServiceProvider.GetRequiredService<ICwsAccountClient>();
      var headers = CustomHeaders();
      var accountsResult = await client.GetMyAccounts(userId, headers);
      var result = await client.GetDeviceLicenses(accountsResult.Accounts[0].Id, headers);

      Assert.IsNotNull(result, "No result from getting device licenses");
      Assert.AreEqual(DeviceLicenseResponseModel.FREE_DEVICE_LICENSE, result.Total);
    }
  }
}
