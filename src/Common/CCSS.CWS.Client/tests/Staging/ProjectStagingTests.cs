using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace CCSS.CWS.Client.UnitTests.Staging
{
  [TestClass]
  public class ProjectStagingTests : BaseTestClass
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
      services.AddTransient<IProjectClient, ProjectClient>();
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
    public async Task Test_CreateProject()
    {
      var client = ServiceProvider.GetRequiredService<IProjectClient>();
      var createProjectRequestModel = new CreateProjectRequestModel
      {             
        accountId = "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97",
        projectName = "my first project",
        timezone = "America/Denver",
        boundary = new ProjectBoundary()
        {
          type = "Polygon",
          coordinates = new List<double[,]>() { { new double[,] { { 150.3, 1.2 }, { 150.4, 1.2 }, { 150.4, 1.3 }, { 150.4, 1.3 }, { 150.3, 1.2 } } } }
        }
      };
      // Forbidden {"status":403,"code":9054,"message":"Forbidden","moreInfo":"Please provide this id to support, while contacting, TraceId 5e7066605642c452c5c580f512629fd1","timestamp":1584424545815} ---> VSS.Common.Exceptions.ServiceException: Forbidden
      var result = await client.CreateProject(createProjectRequestModel, CustomHeaders());

      Assert.IsNotNull(result, "No result from creating my project");
      Assert.IsNotNull(result.Id);
    }
       
  }
}
