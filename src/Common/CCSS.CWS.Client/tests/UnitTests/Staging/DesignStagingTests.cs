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
  public class DesignStagingTests : BaseTestClass
  {

    string stagingCustomerUid = "158ef953-4967-4af7-81cc-952d47cb6c6f"; // "WM TEST TRIMBLECEC MAR 26"
    string myStagingProject = "1c5e016f-77ec-4153-970b-d0cdafe2676c";
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    {
      services.AddSingleton<IWebRequest, GracefulWebRequest>();
      services.AddTransient<ICwsDesignClient, CwsDesignClient>();
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
    public async Task CreateFileTest()
    {
      // test requires a user token.
      var projectUid = new Guid(myStagingProject);
      var createFileRequestModel = new CreateFileRequestModel
      {
        FileName = "myFirstProject.dc"
      };
      var client = ServiceProvider.GetRequiredService<ICwsDesignClient>();

      try
      {
        var result = await client.CreateFile(projectUid, createFileRequestModel, CustomHeaders());
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
  }
}
