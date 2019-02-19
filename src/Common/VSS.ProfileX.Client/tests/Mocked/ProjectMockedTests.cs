using FluentAssertions.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Clients.ProfileX.Interfaces;
using VSS.Common.Abstractions.Clients.ProfileX.Models;
using VSS.ConfigurationStore;
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
    public void Test_CreateProject()
    {
      // The JSON we send must match this, with lowercase names
      // If our C# Model changes, we should still get this JSON
      const string expectedJson =
        "{\"name\":\"Test Project\",\"description\":\"Project Description\",\"startDate\":\"2000-01-01\",\"endDate\":\"2010-12-31\",\"locations\":[{\"type\":\"Home base\",\"primary\":true,\"street\":\"Birmingham Drive\",\"locality\":\"South Island\",\"region\":\"Canterbury\",\"postalCode\":\"8024\",\"country\":\"NZ\",\"latitude\":-43.54509,\"longitude\":172.591805}]}";
      const string expectedId = "trn::profilex:us-west-2:project:08f9d23a-6d55-488b-8fd8-60719203dfcf";

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
        Id = expectedId
      };

      var expectedUrl = $"{baseUrl}/profiles/projects";

      TestRequestSendsCorrectJson(expectedJson, expectedUrl, HttpMethod.Post, createResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<IProjectClient>();
        var result = await client.CreateProject(project);
        
        Assert.IsNotNull(result, "No result from creating a project");
        Assert.AreEqual(result.Id, expectedId);
        return true;
      });
    }

    [TestMethod]
    public void Test_UpdateProject()
    {
      const string expectedJson = "{\"name\": \"Updated Project Name\",\"description\": \"Updated Description\",\"dateTime\": \"2001-01-01\",\"endDate\": \"2001-12-31\",\"locations\": [{\"type\": \"New Home\",\"primary\": true,\"street\": \"12 The Street\",\"locality\": \"North Island\",\"region\": \"Bay of Plenty\",\"postalCode\": \"3123\",\"country\": \"New Zealand\",\"latitude\": -43.54509,\"longitude\": 172.591805}]}";
      const string expectedId = "trn::profilex:us-west-2:project:74A2CFBE-D064-45A6-AA93-ED70C2969BAD";

      var update = new ProjectUpdateRequestModel()
      {
        Name = "Updated Project Name",
        Description = "Updated Description",
        StartDate = new DateTime(2001, 1, 1),
        EndDate = new DateTime(2001, 12, 31),
      };
      update.Locations.Add(new ProjectLocation()
      {
        Country = "New Zealand",
        Locality = "North Island",
        PostalCode = "3123",
        Primary = true,
        Region = "Bay of Plenty",
        Street = "12 The Street",
        Type = "New Home",
        Latitude = -43.545090d,
        Longitude = 172.591805d
      });

      var updateResponse = new ProjectUpdateResponseModel()
      {
        Id = expectedId
      };

      var expectedUrl = $"{baseUrl}/profiles/projects/{expectedId}";

      TestRequestSendsCorrectJson(expectedJson, expectedUrl, HttpMethod.Put, updateResponse, async () =>
      {
        var client = ServiceProvider.GetRequiredService<IProjectClient>();
        var result = await client.UpdateProject(expectedId, update);
        
        Assert.IsNotNull(result, "No result from updating a project, was the mock set up?");
        Assert.AreEqual(result.Id, expectedId);
        return true;
      });
    }

    /// <summary>
    /// Mocks the IWebRequest class to test the proxy class in question. This will ensure the proxy class calls the correct endpoint
    /// And the request object converts to the correct JSON
    /// </summary>
    private void TestRequestSendsCorrectJson<TResponse>(string expectedJson, string url, HttpMethod method, TResponse response, Func<Task<bool>> testExecution)
    {
      var expectedObj = JToken.Parse(expectedJson);
      SetupMockRequest(url,
        () => response,
        validateUrlAction: (requestUrl) => Assert.AreEqual(requestUrl, url),
        validateStreamAction: (requestStream) =>
        {
          requestStream.Seek(0, SeekOrigin.Begin);
          var reader = new StreamReader(requestStream, Encoding.UTF8, false, 1024, true);
          var text = reader.ReadToEnd();

          var actual = JToken.Parse(text);
          actual.Should().BeEquivalentTo(expectedObj);
        },
        validateHttpMethodAction: (requestMethod) =>
        {
          Assert.AreEqual(requestMethod, method, $"Created project should be {method} but it is {requestMethod}");
        }
      );

      var result = testExecution.Invoke().Result;
      Assert.IsTrue(result);
    }

    private void TestRequestSendsCorrectJson(string expectedJson, string url, HttpMethod method, Func<Task<bool>> testExecution)
    {
      var expectedObj = JToken.Parse(expectedJson);
      SetupMockRequest(url,
        validateUrlAction: (requestUrl) => Assert.AreEqual(requestUrl, url),
        validateStreamAction: (requestStream) =>
        {
          requestStream.Seek(0, SeekOrigin.Begin);
          var reader = new StreamReader(requestStream, Encoding.UTF8, false, 1024, true);
          var text = reader.ReadToEnd();

          var actual = JToken.Parse(text);
          actual.Should().BeEquivalentTo(expectedObj);
        },
        validateHttpMethodAction: (requestMethod) =>
        {
          Assert.AreEqual(requestMethod, method, $"Created project should be {method} but it is {requestMethod}");
        }
      );

      var result = testExecution.Invoke().Result;
      Assert.IsTrue(result);
    }

    private void SetupMockRequest<TResponse>(string endpointAddress, Func<TResponse> responseFunc,
      Action<string> validateUrlAction = null, 
      Action<Stream> validateStreamAction = null,
      Action<HttpMethod> validateHttpMethodAction = null)
    {
      mockWebRequest.Setup(s => s.ExecuteRequest<TResponse>(It.Is<string>(v => string.Compare(v, endpointAddress,StringComparison.Ordinal) == 0),
          It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<HttpMethod>(),
          It.IsAny<int?>(),
          It.IsAny<int>(),
          It.IsAny<bool>()))
        .Callback<string, Stream, IDictionary<string,string>, HttpMethod, int?, int, bool>((url, stream, _, method, __, ___, ____) =>
        {
          validateUrlAction?.Invoke(url);

          validateHttpMethodAction?.Invoke(method);

          validateStreamAction?.Invoke(stream);
         })
        .Returns(Task.FromResult(responseFunc()));
    }

    private void SetupMockRequest(string endpointAddress,
      Action<string> validateUrlAction = null, 
      Action<Stream> validateStreamAction = null,
      Action<HttpMethod> validateHttpMethodAction = null)
    {
      mockWebRequest.Setup(s => s.ExecuteRequest(It.Is<string>(v => string.Compare(v, endpointAddress,StringComparison.Ordinal) == 0),
          It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<HttpMethod>(),
          It.IsAny<int?>(),
          It.IsAny<int>(),
          It.IsAny<bool>()))
        .Callback<string, Stream, IDictionary<string,string>, HttpMethod, int?, int, bool>((url, stream, _, method, __, ___, ____) =>
        {
          validateUrlAction?.Invoke(url);

          validateHttpMethodAction?.Invoke(method);

          validateStreamAction?.Invoke(stream);
        })
        .Returns(Task.CompletedTask);
    }
  }
}
