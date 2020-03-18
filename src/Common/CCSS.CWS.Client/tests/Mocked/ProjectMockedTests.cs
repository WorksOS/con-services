using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client.UnitTests.Mocked
{
  [TestClass]
  public class ProjectMockedTests : BaseTestClass
  {
    private string baseUrl;

    private Mock<IWebRequest> mockWebRequest = new Mock<IWebRequest>();

    protected override IServiceCollection SetupTestServices(IServiceCollection services, IConfigurationStore configuration)
    {
      baseUrl = configuration.GetValueString(BaseClient.CWS_URL_KEY);

      services.AddSingleton(mockWebRequest.Object);
      services.AddTransient<IProjectClient, ProjectClient>();

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
      var expectedUrl = $"{baseUrl}/projects";

      MockUtilities.TestRequestSendsCorrectJson("Create a project", mockWebRequest, null, expectedUrl, HttpMethod.Post, createProjectResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<IProjectClient>();
        var result = await client.CreateProject(createProjectRequestModel);

        Assert.IsNotNull(result, "No result from posting my project");
        Assert.AreEqual(result.Id, expectedId);
        return true;
      });
    }

  }
}
