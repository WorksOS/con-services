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
  public class UserMockedTests : BaseTestClass
  {
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    {
      services.AddSingleton(mockWebRequest.Object);
      services.AddSingleton(mockServiceResolution.Object);
      services.AddTransient<ICwsUserClient, CwsUserClient>();

      return services;
    }

    [TestMethod]
    public void Test_GetUser()
    {
      const string userId = "trn::profilex:us-west-2:user:d79a392d-6513-46c1-baa1-75c537cf0c32";
      const string expectedId = userId;
      const string expectedLanguage = "en-US";
      const string route = "/users/me";

      var userModel = new UserResponseModel
      {
       Id = expectedId,
       Language = expectedLanguage,
       FirstName = "Joe",
       LastName = "Bloggs",
       Email = "someone@somewhere.com"
      };

      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get User", mockWebRequest, null, expectedUrl, HttpMethod.Get, userModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsUserClient>();
        var result = await client.GetUser(userId);

        Assert.IsNotNull(result, "No result from getting user");
        Assert.AreEqual(result.Id, expectedId);
        Assert.AreEqual(result.Language, expectedLanguage);
        return true;
      });
    }
  }
}
