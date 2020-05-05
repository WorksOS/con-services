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
using System.Linq;

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

    [Fact(Skip = "manual testing only")]
    public async Task GetMyAccountsTest()
    {
      var client = ServiceProvider.GetRequiredService<ICwsAccountClient>();
      var expectedAccountId = "158ef953-4967-4af7-81cc-952d47cb6c6f";
      var result = await client.GetMyAccounts(TRNHelper.ExtractGuid(userId).Value, CustomHeaders());

      Assert.NotNull(result);
      Assert.NotNull(result.Accounts);
      Assert.True(result.Accounts.Count > 0);     
      Assert.Single(result.Accounts.Where(a => string.Compare(a.Id, expectedAccountId, true) == 0));
    }
       
   [Fact(Skip = "manual testing only")]
   //[Fact]
    public async Task GetDeviceAccountsTest()
    {
      var client = ServiceProvider.GetRequiredService<ICwsAccountClient>();
      var headers = CustomHeaders();
      var accountId = "158ef953-4967-4af7-81cc-952d47cb6c6f";

      var result = await client.GetDeviceLicenses(new Guid(accountId), headers);
      Assert.NotNull(result);
      Assert.Equal(DeviceLicenseResponseModel.FREE_DEVICE_LICENSE, result.Total);
    }
  }
}
