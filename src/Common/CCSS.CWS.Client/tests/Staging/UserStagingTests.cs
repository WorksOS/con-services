using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.ServiceDiscovery;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace CCSS.CWS.Client.UnitTests.Staging
{
  [TestClass]
  public class UserStagingTests : BaseTestClass
  {
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    {
      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddTransient<ICwsUserClient, CwsUserClient>();
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
    [Ignore]
    public async Task Test_GetUser()
    {
      var client = ServiceProvider.GetRequiredService<ICwsUserClient>();
      //todoMaaverick. Currently this will fail. Requires a user token, not application token and userId should come from the JWT.
      var result = await client.GetUser(userId, CustomHeaders());

      Assert.IsNotNull(result, "No result from getting user");
      Assert.AreEqual(userId, result.Id);
    }
  }
}
