using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;
using VSS.Common.ServiceDiscovery;
using Xunit;

namespace CCSS.CWS.Client.UnitTests.Staging
{
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

    [Fact(Skip = "Can be used for testing and debugging")]
    public async Task GetMyAccountsTest()
    {
      var client = ServiceProvider.GetRequiredService<ICwsAccountClient>();
      var result = await client.GetMyAccounts(TRNHelper.ExtractGuid(userId).Value, CustomHeaders());

      Assert.NotNull(result);
      Assert.NotNull(result.Accounts);
      Assert.True(result.Accounts.Count > 0);
    }

    [Fact(Skip = "Implement test when we have the endpoint in cws")]
    public async Task GetAccountsForUserTest()
    {
      throw new NotImplementedException();
    }

    [Fact(Skip = "Implement test when we have the endpoint in cws")]
    public async Task GetAccountForUserTest()
    {
      throw new NotImplementedException();
    }

    [Fact(Skip = "Can be used for testing and debugging")]
    public async Task GetDeviceAccountsTest()
    {
      var client = ServiceProvider.GetRequiredService<ICwsAccountClient>();
      var headers = CustomHeaders();
      var accountsResult = await client.GetMyAccounts(TRNHelper.ExtractGuid(userId).Value, headers);
      Assert.NotNull(accountsResult);
      Assert.True(accountsResult.Accounts.Count > 0);

      var result = await client.GetDeviceLicenses(new Guid(accountsResult.Accounts[0].Id), headers);
      Assert.NotNull(result);
      Assert.Equal(DeviceLicenseResponseModel.FREE_DEVICE_LICENSE, result.Total);
    }
  }
}
