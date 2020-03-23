﻿using System;
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
  public class ProjectStagingTests : BaseTestClass
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
      services.AddTransient<ICwsProjectClient, CwsProjectClient>();
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
        // todoMaverick
        var tokenUsername = ""; //  "jeannnie_may@trimble.com";
        var tokenPassWord = "";
        var grantType = $"grant_type=password&username={tokenUsername}&password={tokenPassWord}";
        var token = ServiceProvider.GetService<ITPaaSApplicationAuthentication>().GetUserBearerToken(grantType);
        return !string.IsNullOrEmpty(token);
      }
      catch (Exception e)
      {
        // No point running the tests if tpass is offline or not authenticating
        return false;
      }
    }

    [TestMethod]
    [Ignore] // todoMaverick this fails as don't have a good user name to get token, but don't want to insert projects all the time anyway
             // (requires a user token. This is ok as will have one via from ProjectSvc) 
    public async Task Test_CreateProject()
    {
      var accountClient = ServiceProvider.GetRequiredService<ICwsAccountClient>();
      var accountListResponseModel = await accountClient.GetMyAccounts(CustomHeaders());
      Assert.IsNotNull(accountListResponseModel, "No result from getting my accounts");
      Assert.IsTrue(accountListResponseModel.Accounts.Count > 0);

      var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
      var createProjectRequestModel = new CreateProjectRequestModel
      {             
        accountId = accountListResponseModel.Accounts[0].Id,
        projectName = $"Merino test project {Guid.NewGuid()}",
        timezone = "America/Denver",
        boundary = new ProjectBoundary()
        {
          type = "POLYGON",
          coordinates = new List<double[,]>() { { new double[,] { { 150.3, 1.2 }, { 150.4, 1.2 }, { 150.4, 1.3 }, { 150.4, 1.3 }, { 150.3, 1.2 } } } }
        }
      };
      var payload = JsonConvert.SerializeObject(createProjectRequestModel);

      // Forbidden {"status":403,"code":9054,"message":"Forbidden","moreInfo":"Please provide this id to support, while contacting, TraceId 5e7066605642c452c5c580f512629fd1","timestamp":1584424545815} ---> VSS.Common.Exceptions.ServiceException: Forbidden
      var result = await client.CreateProject(createProjectRequestModel, CustomHeaders());

      Assert.IsNotNull(result, "No result from creating my project");
      Assert.IsNotNull(result.Id);
    }
       
  }
}
