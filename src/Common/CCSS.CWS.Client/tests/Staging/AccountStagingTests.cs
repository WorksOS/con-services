using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace CCSS.CWS.Client.UnitTests.Staging
{
  [TestClass]
  public class AccountStagingTests : BaseTestClass
  {
    private static string authHeader = string.Empty;

    private IConfigurationStore configuration;
    private ITPaaSApplicationAuthentication authentication;

    private string baseUrl;

    protected override IServiceCollection SetupTestServices(IServiceCollection services, IConfigurationStore configuration)
    {
      this.configuration = configuration;
      baseUrl = configuration.GetValueString(BaseClient.CWS_URL_KEY);

      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddTransient<IAccountClient, AccountClient>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();
      services.AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();

      return services;
    }

    protected override async Task<bool> PretestChecks()
    {
      if (string.IsNullOrEmpty(baseUrl))
      {
        Log.Fatal("No URL set for CWS");
        return false;
      }

      // Get Bearer Token
      try
      {
        var token = ServiceProvider.GetService<ITPaaSApplicationAuthentication>().GetApplicationBearerToken();
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
      var result = await client.GetMyAccounts(CustomHeaders());

      Assert.IsNotNull(result, "No result from getting my accounts");
      Assert.IsNotNull(result.Accounts);
      Assert.IsTrue(result.Accounts.Count > 0);
    }

    [TestMethod]
    public async Task Test_GetDeviceAccounts()
    {
      var client = ServiceProvider.GetRequiredService<IAccountClient>();
      var headers = CustomHeaders();
      var accountsResult = await client.GetMyAccounts(headers);
      var result = await client.GetDeviceLicenses(accountsResult.Accounts[0].Id, headers);

      Assert.IsNotNull(result, "No result from getting device licenses");
      Assert.AreEqual(DeviceLicenseResponseModel.FREE_DEVICE_LICENSE, result.Total);
    }
  }
}
