using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.ProfileX.Interfaces;
using VSS.Common.Abstractions.Clients.ProfileX.Models;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.ProfileX.Client.UnitTests.Mocked
{
  [TestClass]
  public class ProjectMockedTests : BaseTestClass
  {
    private string baseUrl;

    private Mock<IWebRequest> mockWebRequest = new Mock<IWebRequest>();
    


    protected override IServiceCollection SetupTestServices(IServiceCollection services, IConfigurationStore configuration)
    {
      baseUrl = configuration.GetValueString(BaseClient.PROFILE_X_URL_KEY);

      services.AddSingleton(mockWebRequest.Object);
      services.AddTransient<IProjectClient, ProjectClient>();

      return services;
    }

    [TestMethod]
    public async Task Test_CreateProject()
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

      var createResponseModel = new ProjectCreateResponseModel()
      {
        Id = "trn::profilex:us-west-2:project:08f9d23a-6d55-488b-8fd8-60719203dfcf"
      };

      var expectedUrl = $"{baseUrl}/profiles/projects";

      mockWebRequest.Setup(s => s.ExecuteRequest<ProjectCreateResponseModel>(It.IsAny<string>(),
          It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<HttpMethod>(),
          It.IsAny<int?>(),
          It.IsAny<int>(),
          It.IsAny<bool>()))
        .Callback<string, Stream, IDictionary<string,string>, HttpMethod, int?, int, bool>((url, stream, _, method, __, ___, ____) =>
        {
            Assert.AreEqual(url, expectedUrl);
            Assert.AreEqual(method, HttpMethod.Post, $"Created project should be {HttpMethod.Post} but it is {method}");

            stream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(stream, Encoding.UTF8, false,1024, true);
            var text = reader.ReadToEnd();

            var obj = JsonConvert.DeserializeObject(text);

            throw new NotImplementedException("Finish off these tests");
        })
        .Returns(Task.FromResult(createResponseModel));

      var client = ServiceProvider.GetRequiredService<IProjectClient>();
      var result = await client.CreateProject(project);


      Assert.IsNotNull(result, "No result from creating a project");

    }

  }
}
