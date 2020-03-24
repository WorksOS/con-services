using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;

namespace CCSS.CWS.Client.UnitTests.Mocked
{
  [TestClass]
  public class ProjectMockedTests : BaseTestClass
  {
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    {
      services.AddSingleton(mockWebRequest.Object);
      services.AddSingleton(mockServiceResolution.Object);
      services.AddTransient<ICwsProjectClient, CwsProjectClient>();

      return services;
    }

    [TestMethod]
    public void Test_CreateProject()
    {
      const string expectedId = "trn::profilex:us-west-2:project:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";

      var createProjectRequestModel = new CreateProjectRequestModel
      {
        accountId = "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97",
        projectName = "my first project",
        timezone = "America/Denver",
        boundary = new ProjectBoundary()
        {
          type = "Polygon",
          coordinates = new List<double[,]>() { { new double[2, 2] { { 180, 90 }, { 180, 90 } } } } 
        }
      };

      var createProjectResponseModel = new CreateProjectResponseModel
      {
        Id = expectedId
      };
      const string route = "/projects";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Create a project", mockWebRequest, null, expectedUrl, HttpMethod.Post, createProjectResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
        var result = await client.CreateProject(createProjectRequestModel);

        Assert.IsNotNull(result, "No result from posting my project");
        Assert.AreEqual(result.Id, expectedId);
        return true;
      });
    }

  }
}
