using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.Clients.ProfileX.Interfaces;
using VSS.Common.Abstractions.Clients.ProfileX.Models;
using VSS.Common.Abstractions.Http;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.ProfileX.Client.UnitTests.Staging
{
  [TestClass]
  public class ProjectStagingTests : BaseTestClass
  {
    private static string authHeader = string.Empty;

    private IConfigurationStore configuration;

    private string baseUrl;

    protected override IServiceCollection SetupTestServices(IServiceCollection services, IConfigurationStore configuration)
    {
      this.configuration = configuration;
      baseUrl = configuration.GetValueString(BaseClient.PROFILE_X_URL_KEY);

      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddTransient<IProjectClient, ProjectClient>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();

      return services;
    }

    protected override async Task<bool> PretestChecks()
    {
      if (string.IsNullOrEmpty(baseUrl))
      {
        Log.LogCritical("No URL set for Profile X");
        return false;
      }

      // Get Bearer Token
      var customHeaders = new Dictionary<string, string>
      {
        {HeaderConstants.ACCEPT, ContentTypeConstants.APPLICATION_JSON},
        {HeaderConstants.CONTENT_TYPE, ContentTypeConstants.X_WWW_FORM_URLENCODED},
      };

      var tpaas = ServiceProvider.GetService<ITPaasProxy>();

      try
      {

        Assert.Inconclusive("TODO Add User authentication for Profile X");
//        var tPaasOauthResult = await tpaas.GetApplicationBearerToken("client_credentials", 
//          configuration.GetValueString("PROFILE_X_API_CLIENT_ID"), 
//          configuration.GetValueString("PROFILE_X_API_CLIENT_SECRET"),
//          customHeaders);
//
//        if (tPaasOauthResult.Code == 0)
//        {
//          authHeader = $"Bearer {tPaasOauthResult.tPaasOauthRawResult.access_token}";
          return true;
//        }
      }
      catch
      {
        // No point running the tests if tpass is offline or not authenticating
        return false;
      }

      return false;
    }

    [TestMethod]
    public async Task tewtwetrwe()
    {


      var project = new ProjectCreateRequestModel()
      {
        Name = $"Test Project",
        Description = "Project Description",
        StartDate = new DateTime(2000, 1, 1),
        EndDate = new DateTime(2010, 12, 31)
      };
      project.Locations.Add(new ProjectLocation()
      {
        Country = "NZ",
        Locality = "South Island",
        PostalCode = "8024",
        Primary = true,
        Region = "Canterbury",
        Street = "Birmingham Drive",
        Type = "Home base",
        Latitude = -43.545090d,
        Longitude = 172.591805d
      });

      var client = ServiceProvider.GetRequiredService<IProjectClient>();
//      var result = await client.RetrieveMyProjects(customHeaders: GetCustomerHeaders());
      var result = await client.CreateProject(project, GetCustomerHeaders());

      Assert.IsNotNull(result);
    }



    
    private Dictionary<string, string> GetCustomerHeaders()
    {
      if (string.IsNullOrEmpty(authHeader))
      {
        Assert.Inconclusive("No TPaaS Token, cannot run tests");
        return new Dictionary<string, string>();
      }
      else
      {
        return new Dictionary<string, string>()
        {
          {HeaderConstants.ACCEPT, ContentTypeConstants.APPLICATION_JSON},
          {HeaderConstants.CONTENT_TYPE, ContentTypeConstants.APPLICATION_JSON},
          {HeaderConstants.AUTHORIZATION, authHeader}
        };
      }
    }
  }
}