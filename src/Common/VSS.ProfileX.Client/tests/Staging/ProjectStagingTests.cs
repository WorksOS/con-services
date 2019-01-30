using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.Clients.ProfileX.Interfaces;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.ProfileX.Client.UnitTests.Staging
{
  [TestClass]
  public class ProjectStagingTests : BaseTestClass
  {
    private string baseUrl;

    protected override IServiceCollection SetupTestServices(IServiceCollection services, IConfigurationStore configuration)
    {
      baseUrl = configuration.GetValueString(BaseClient.PROFILE_X_URL_KEY);

      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddTransient<IProjectClient, ProjectClient>();

      return services;
    }

    protected override bool PretestChecks()
    {
      if (string.IsNullOrEmpty(baseUrl))
      {
        Log.LogCritical("No URL set for Profile X");
        return false;
      }

      // TODO check to see if online
      return true;
    }

    [TestMethod]
    public void tewtwetrwe()
    {
      var token = "GET TOKEN";
      var headers = new Dictionary<string, string>
      {
        {"Authorization", token}
      };


      var client = ServiceProvider.GetService<IProjectClient>();
      var result = client.RetrieveMyProjects(customHeaders: headers).Result;


      Assert.IsNotNull(result);
    }
  }
}