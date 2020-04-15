using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;
using VSS.Common.ServiceDiscovery;
using Xunit;

namespace CCSS.CWS.Client.UnitTests.Staging
{
  public class ProjectStagingTests : BaseTestClass
  {
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

    [Fact(Skip= "test waiting on C2S3CON-211")]
    public async Task CreateProjectTest()
    {
      // test requires a user token.
      var accountClient = ServiceProvider.GetRequiredService<ICwsAccountClient>();
      var accountListResponseModel = await accountClient.GetMyAccounts(TRNHelper.ExtractGuid(userId).Value, CustomHeaders());
      Assert.NotNull(accountListResponseModel);
      Assert.True(accountListResponseModel.Accounts.Count > 0);

      var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
      var createProjectRequestModel = new CreateProjectRequestModel
      {             
        accountId = accountListResponseModel.Accounts[0].Id,
        projectName = $"Merino test project {Guid.NewGuid()}",
        boundary = new ProjectBoundary()
        {
          type = "POLYGON",
          coordinates = new List<double[,]>() { { new double[,] { { 150.3, 1.2 }, { 150.4, 1.2 }, { 150.4, 1.3 }, { 150.4, 1.4 }, { 150.3, 1.2 } } } }
        }
      };
      var payload = JsonConvert.SerializeObject(createProjectRequestModel);

      // Forbidden {"status":403,"code":9054,"message":"Forbidden","moreInfo":"Please provide this id to support, while contacting, TraceId 5e7066605642c452c5c580f512629fd1","timestamp":1584424545815} ---> VSS.Common.Exceptions.ServiceException: Forbidden
      var result = await client.CreateProject(createProjectRequestModel, CustomHeaders());

      Assert.NotNull(result);
      Assert.NotNull(result.Id);
    }

    [Fact(Skip = "Implement test when we have the endpoint in cws")]
    public void UpdateProjectDetailsTest()
    {
      throw new NotImplementedException();
    }

    [Fact(Skip = "Implement test when we have the endpoint in cws")]
    public void UpdateProjectBoundaryTest()
    {
      throw new NotImplementedException();
    }

    }
}
