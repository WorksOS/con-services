using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using VSS.TRex.HttpClients.Models.Responses;
using VSS.TRex.HttpClients.RequestHandlers;
using Xunit;

namespace VSS.TRex.HttpClients.Tests
{
  public class TPaaSAppCredsAuthenticatedRequestHandlerTests
  {
    /// <summary>
    /// TPaaSApplicationCredentialsRequestHandler presensts a basic token
    /// and expects to recieve a bearer token and expiry time from TPaaS
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task TestApplicationAuthHeaderIsAdded()
    {
      var bearertokenType = "Bearer";

      var accessToken = "token";
      var tokenExpiresIn = 100;
      var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
      handlerMock
         .Protected()
         // Setup the PROTECTED method to mock
         .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(request =>
            request.Headers.Authorization.ToString() == $"Basic {accessToken}"
            ),
            ItExpr.IsAny<CancellationToken>()
         )
         // prepare the expected response of the mocked http call
         .ReturnsAsync((HttpRequestMessage req, CancellationToken token) =>
         {
           return new HttpResponseMessage()
           {
             StatusCode = HttpStatusCode.OK,
             Content = new StringContent(JsonConvert.SerializeObject(new TPaaSClientCredentialsRawResponse
             {
               TokenType = bearertokenType,
               TokenExpiry = tokenExpiresIn,
               AccessToken = accessToken
             })),
             RequestMessage = req
           };
         })
         .Verifiable();

      var authHandler = new TPaaSApplicationCredentialsRequestHandler(handlerMock.Object);
      authHandler.SetTPaasToken(accessToken);

      // use real http client with mocked handler here
      var httpClient = new HttpClient(authHandler)
      {
        BaseAddress = new Uri("http://nowhere.specific/"),
      };

      var result = await httpClient.PostAsync("", new StringContent(string.Empty));

      result.Should().NotBeNull();

      //Check that the middleware is adding the header and calling the correct methods
      handlerMock.Protected().Verify(
          "SendAsync",
          Times.Once(), // we expected a single request
          ItExpr.Is<HttpRequestMessage>(arg =>
            arg.Headers.Authorization.ToString() == $"Basic {accessToken}"
         ),
          ItExpr.IsAny<CancellationToken>()
       );
      var resultObject = JsonConvert.DeserializeObject<TPaaSClientCredentialsRawResponse>(result.Content.ReadAsStringAsync().Result);
      resultObject.TokenType.Should().Be(bearertokenType);
      resultObject.AccessToken.Should().Be(accessToken);
      resultObject.TokenExpiry.Should().Be(tokenExpiresIn);
    }
  }
}
