using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace CCSS.CWS.Client.UnitTests.Staging
{
  [TestClass]
  public class DeviceStagingTests : BaseTestClass
  {
    private static string authHeader = string.Empty;

    private IConfigurationStore configuration;
    private ITPaaSApplicationAuthentication authentication;

    private string baseUrl;

    protected override IServiceCollection SetupTestServices(IServiceCollection services, IConfigurationStore configuration)
    {
      this.configuration = configuration;
      baseUrl = configuration.GetValueString(BaseClient.CWS_PROFILEMANAGER_URL_KEY);

      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddTransient<ICwsAccountClient, CwsAccountClient>();
      services.AddTransient<ICwsDeviceClient, CwsDeviceClient>();
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
    [Ignore] // todoMaverick does this need a user token Sankari? 
             // (requires a user token. This is ok as will have one via from ProjectSvc) 
    public async Task Test_GetDevicesForAccount()
    {
      var accountClient = ServiceProvider.GetRequiredService<ICwsAccountClient>();
      var accountListResponseModel = await accountClient.GetMyAccounts(CustomHeaders());
      Assert.IsNotNull(accountListResponseModel, "No result from getting my accounts");
      Assert.IsTrue(accountListResponseModel.Accounts.Count > 0);

      var client = ServiceProvider.GetRequiredService<ICwsDeviceClient>();
      var deviceList = await client.GetDevicesForAccount(accountListResponseModel.Accounts[0].Id, CustomHeaders());

      Assert.IsNotNull(deviceList, "No result from getting device list");
      Assert.IsTrue(deviceList.Devices.Count > 0);
    }
       
  }
}
