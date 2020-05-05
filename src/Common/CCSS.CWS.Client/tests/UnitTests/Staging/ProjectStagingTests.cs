using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;
using VSS.Common.ServiceDiscovery;
using Xunit;
using System.Net;

namespace CCSS.CWS.Client.UnitTests.Staging
{
  public class ProjectStagingTests : BaseTestClass
  {

    string stagingCustomerUid = "158ef953-4967-4af7-81cc-952d47cb6c6f"; // "WM TEST TRIMBLECEC MAR 26"
    string mystagingProject = "1c5e016f-77ec-4153-970b-d0cdafe2676c";
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    {
      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddTransient<ICwsAccountClient, CwsAccountClient>();
      services.AddTransient<ICwsProjectClient, CwsProjectClient>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();
      services.AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();
      services.AddServiceDiscovery();

      return services;
    }

    protected override bool PretestChecks()
    {
      return CheckTPaaS();
    }

    [Fact(Skip= "manual testing only")]
    public async Task CreateProjectTest()
    {
      // test requires a user token.
      var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
      var createProjectRequestModel = new CreateProjectRequestModel
      {             
        AccountId = stagingCustomerUid,
        ProjectName = $"Merino test project {Guid.NewGuid()}",
        Timezone = "New Zealand Standard Time",
        Boundary = new ProjectBoundary()
        {
          type = "POLYGON",
          coordinates = new List<double[,]>() { { new double[,]
                { { 172.606, -43.574 },
                  { 172.606, -43.574 },
                  { 172.614, -43.578 },
                  { 172.615, -43.577 },
                  { 172.617, -43.573 },
                  { 172.610, -43.570 },
                  { 172.606, -43.574 }
                }} }
        }
      };

      try
      {
        var result = await client.CreateProject(createProjectRequestModel, CustomHeaders());
      }
      catch(Exception e)
      {
        Assert.Contains(HttpStatusCode.BadRequest.ToString(), e.Message);
        Assert.Contains(":9056,", e.Message);
        /*
         * This call returns: (possibly because this is application context?) 
         * BadRequest {"status":400,"code":9056,"message":"Bad request","moreInfo":"Please provide this id to support, while contacting, TraceId 5e9a82a7ad9caf287518ec873da80845","timestamp":1587184295988}
         * 
         * Postman returns:
         * {
            "status": 400,
            "code": 9006,
            "message": "Account not found or not active",
            "moreInfo": "Please provide this id to support, while contacting, TraceId 5e9a80357074c0914b66ae9cdcf6dfa7",
            "timestamp": 1587183669699,
            "fieldErrors": [
                {
                    "field": "accountId",
                    "attemptedValue": "trn::profilex:us-west-2:account:158ef953-4967-4af7-81cc-952d47cb6c6"
                }
            ]
        }
        */

      }

      //Assert.NotNull(result);
      //Assert.NotNull(result.Id);
    }

    [Fact(Skip = "manual testing only")]
    public async Task UpdateProjectDetailsTest()
    {
      // test requires a user token.
      var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
      var updateProjectDetailsRequestModel = new UpdateProjectDetailsRequestModel
      {        
        projectName = $"Merino test project {Guid.NewGuid()}"
      };

      try
      {
        await client.UpdateProjectDetails(new Guid(mystagingProject), updateProjectDetailsRequestModel, CustomHeaders());
      }
      catch (Exception e)
      {
        Assert.Contains(HttpStatusCode.BadRequest.ToString(), e.Message);
        Assert.Contains(":9056,", e.Message);
        /*
         * This call returns: (possibly because this is application context?) 
         * BadRequest {"status":400,"code":9056,"message":"Bad request","moreInfo":"Please provide this id to support, while contacting, TraceId 5e9a82a7ad9caf287518ec873da80845","timestamp":1587184295988}
         * 
         * Postman returns:
         * {
            "status": 400,
            "code": 9006,
            "message": "Account not found or not active",
            "moreInfo": "Please provide this id to support, while contacting, TraceId 5e9a80357074c0914b66ae9cdcf6dfa7",
            "timestamp": 1587183669699,
            "fieldErrors": [
                {
                    "field": "accountId",
                    "attemptedValue": "trn::profilex:us-west-2:account:158ef953-4967-4af7-81cc-952d47cb6c6"
                }
            ]
        }
        */

      }

      //Assert.NotNull(result);
      //Assert.NotNull(result.Id);
    }

    [Fact(Skip = "manual testing only")]
    public async Task UpdateProjectBoundaryTest()
    {
      // test requires a user token.
      var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
      var boundary = new ProjectBoundary()
        {
          type = "POLYGON",
          coordinates = new List<double[,]>() { { new double[,]
                { { 172.606, -43.574 },
                  { 172.606, -43.574 },
                  { 172.614, -43.578 },
                  { 172.615, -43.577 },
                  { 172.617, -43.573 },
                  { 172.610, -43.570 },
                  { 172.606, -43.574 }
                }} }
      };

      try
      {
        await client.UpdateProjectBoundary(new Guid(mystagingProject), boundary, CustomHeaders());
      }
      catch (Exception e)
      {
        Assert.Contains(HttpStatusCode.BadRequest.ToString(), e.Message);
        Assert.Contains(":9056,", e.Message);
        /*
         * This call returns: (possibly because this is application context?) 
         * BadRequest {"status":400,"code":9056,"message":"Bad request","moreInfo":"Please provide this id to support, while contacting, TraceId 5e9a82a7ad9caf287518ec873da80845","timestamp":1587184295988}
         * 
         * Postman returns:
         * {
            "status": 400,
            "code": 9006,
            "message": "Account not found or not active",
            "moreInfo": "Please provide this id to support, while contacting, TraceId 5e9a80357074c0914b66ae9cdcf6dfa7",
            "timestamp": 1587183669699,
            "fieldErrors": [
                {
                    "field": "accountId",
                    "attemptedValue": "trn::profilex:us-west-2:account:158ef953-4967-4af7-81cc-952d47cb6c6"
                }
            ]
        }
        */

      }

      //Assert.NotNull(result);
      //Assert.NotNull(result.Id);
    }

  }
}
