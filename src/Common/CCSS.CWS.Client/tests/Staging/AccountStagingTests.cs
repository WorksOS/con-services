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
using VSS.Common.Abstractions.Clients.CWS.Utilities;

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
    public async Task GetMyAccountsTest()
    {
      var client = ServiceProvider.GetRequiredService<ICwsAccountClient>();
      var result = await client.GetMyAccounts(TRNHelper.ExtractGuid(userId).Value, CustomHeaders());

      Assert.IsNotNull(result, "No result from getting my accounts");
      Assert.IsNotNull(result.Accounts);
      Assert.IsTrue(result.Accounts.Count > 0);
    }

    [TestMethod]
    [Ignore("Implement test when we have the endpoint in cws")]
    public async Task GetAccountsForUserTest()
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    [Ignore("Implement test when we have the endpoint in cws")]
    public async Task GetAccountForUserTest()
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    public async Task GetDeviceAccountsTest()
    {
      var client = ServiceProvider.GetRequiredService<ICwsAccountClient>();
      var headers = CustomHeaders();
      var accountsResult = await client.GetMyAccounts(TRNHelper.ExtractGuid(userId).Value, headers);
      Assert.IsNotNull(accountsResult);
      Assert.IsTrue(accountsResult.Accounts.Count > 0);

      var result = await client.GetDeviceLicenses(new Guid(accountsResult.Accounts[0].Id), headers);
      Assert.IsNotNull(result, "No result from getting device licenses");
      Assert.AreEqual(DeviceLicenseResponseModel.FREE_DEVICE_LICENSE, result.Total);
    }
  }
}
