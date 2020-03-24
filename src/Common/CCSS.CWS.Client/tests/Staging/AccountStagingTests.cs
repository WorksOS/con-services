using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Cache.MemoryCache;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;
using VSS.Common.ServiceDiscovery;

namespace CCSS.CWS.Client.UnitTests.Staging
{
  [TestClass]
  public class AccountStagingTests : BaseTestClass
  {
    private static string authHeader = string.Empty;

    private IConfigurationStore configuration;
    private ITPaaSApplicationAuthentication authentication;

    protected override IServiceCollection SetupTestServices(IServiceCollection services, IConfigurationStore configuration)
    {
      this.configuration = configuration;

      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddTransient<IAccountClient, AccountClient>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();
      services.AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();
      services.AddSingleton<IMemoryCache, MemoryCache>();
      services.AddSingleton<IDataCache, InMemoryDataCache>();
      services.AddServiceDiscovery();

      return services;
    }

    protected override async Task<bool> PretestChecks()
    {
      // Get Bearer Token
      try
      {
        var token = ServiceProvider.GetService<ITPaaSApplicationAuthentication>().GetApplicationBearerToken();
        //todomaverick
        //TODO: set userId from JWT
        //string authorization = context.Request.Headers["X-Jwt-Assertion"];
        //var jwtToken = new TPaaSJWT(authorization);
        //userId = jwtToken.IsApplicationToken ? jwtToken.ApplicationId : jwtToken.UserUid.ToString();
        //TEMPORARY use Elspeth's userId
        userId = "trn::profilex:us-west-2:user:d79a392d-6513-46c1-baa1-75c537cf0c32";
        //NOTE: TPaaS should return Id_token (JWT) as well as Access_token for GetApplicationBearerToken but no longer appears to support this
        return !string.IsNullOrEmpty(token);
      }
      catch (Exception e)
      {
        // No point running the tests if tpass is offline or not authenticating
        return false;
      }
    }

    [TestMethod]
    public async Task Test_GetMyAccounts()
    {
      var client = ServiceProvider.GetRequiredService<IAccountClient>();
      var result = await client.GetMyAccounts(userId, CustomHeaders());

      Assert.IsNotNull(result, "No result from getting my accounts");
      Assert.IsNotNull(result.Accounts);
      Assert.IsTrue(result.Accounts.Count > 0);
    }

    [TestMethod]
    public async Task Test_GetDeviceAccounts()
    {
      var client = ServiceProvider.GetRequiredService<IAccountClient>();
      var headers = CustomHeaders();
      var accountsResult = await client.GetMyAccounts(userId, headers);
      var result = await client.GetDeviceLicenses(accountsResult.Accounts[0].Id, headers);

      Assert.IsNotNull(result, "No result from getting device licenses");
      Assert.AreEqual(DeviceLicenseResponseModel.FREE_DEVICE_LICENSE, result.Total);
    }
  }
}
