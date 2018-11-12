using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using VSS.TRex.HttpClients.Clients;
using VSS.TRex.HttpClients.Models.Responses;
using VSS.TRex.HttpClients.RequestHandlers;
using Xunit;
using MoqExtensions;
using VSS.TRex.HttpClients.Models;

namespace VSS.TRex.HttpClients.Tests
{
  public class TPaaSClientTests
  {


    /// <summary>
    /// TPaaSClient should cache a token until it expires
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task TestApplicationBearerTokenIsCached()
    {
      var bearertokenType = "Bearer";

      var accessToken = "token";
      var tokenExpiresIn = 100;
      var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
      handlerMock
         .Protected()
         // Setup the protected method to mock
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



      var tpaasClient = new TPaaSClient(httpClient, new Mock<ILogger<TPaaSClient>>().Object);
      var clientState = new Mock<ITPaaSClientState>().SetupAllProperties();
      await tpaasClient.setState(clientState.Object);

      var bearerToken = await tpaasClient.GetBearerTokenAsync();
      var bearerToken2 = await tpaasClient.GetBearerTokenAsync();

      bearerToken.Should().Be(bearerToken2);

      //Check that the client calling the correct methods
      //This should only be called once
      handlerMock.Protected().Verify(
          "SendAsync",
          Times.Once(), // we expected a single request
          ItExpr.Is<HttpRequestMessage>(arg =>
            arg.Headers.Authorization.ToString() == $"Basic {accessToken}"
         ),
          ItExpr.IsAny<CancellationToken>()
       );
    }


    /// <summary>
    /// TPaaSClient should cache a token until it is 60 seconds away from expiring.
    /// At which point it should revoke it and refresh it.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task TestApplicationBearerTokenCacheExpires()
    {
      var bearertokenType = "Bearer";
      var accessToken = "token";
      var tokenExpiresIn = 100;
      var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
      handlerMock
         .Protected()
         // Setup the protected method to mock calls to /token
         .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(request =>
            request.RequestUri.ToString().Contains("/token") &&
            request.Headers.Authorization.ToString() == $"Basic {accessToken}"
            ),
            ItExpr.IsAny<CancellationToken>()
         )
         .ReturnsInOrder(
            Task.FromResult(new HttpResponseMessage()
            {
              StatusCode = HttpStatusCode.OK,
              Content = new StringContent(JsonConvert.SerializeObject(new TPaaSClientCredentialsRawResponse
              {
                TokenType = bearertokenType,
                TokenExpiry = 50,
                AccessToken = accessToken
              }))
            }),
             Task.FromResult(new HttpResponseMessage()
             {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(JsonConvert.SerializeObject(new TPaaSClientCredentialsRawResponse
               {
                 TokenType = bearertokenType,
                 TokenExpiry = tokenExpiresIn,
                 AccessToken = accessToken + '2'
               }))
             })
        );

      handlerMock
         .Protected()
         // Setup the protected method to mock calls to /revoke
         .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(request =>
            request.RequestUri.ToString().Contains("/revoke") &&
            request.Headers.Authorization.ToString() == $"Basic {accessToken}"
            ),
            ItExpr.IsAny<CancellationToken>()
         )
         .ReturnsAsync(
            new HttpResponseMessage()
            {
              StatusCode = HttpStatusCode.OK,
            }
        )
        .Verifiable();

      var authHandler = new TPaaSApplicationCredentialsRequestHandler(handlerMock.Object);
      authHandler.SetTPaasToken(accessToken);

      // use real http client with mocked handler here
      var httpClient = new HttpClient(authHandler)
      {
        BaseAddress = new Uri("http://nowhere.specific/"),
      };

      var tpaasClient = new TPaaSClient(httpClient, new Mock<ILogger<TPaaSClient>>().Object);
      var clientState = new Mock<ITPaaSClientState>().SetupAllProperties();
      await tpaasClient.setState(clientState.Object);


      var bearerToken = await tpaasClient.GetBearerTokenAsync();
      var bearerToken2 = await tpaasClient.GetBearerTokenAsync();


      bearerToken.Should().Be($"{bearertokenType} {accessToken}");
      bearerToken2.Should().Be($"{bearertokenType} {accessToken}2");

      //Check that the client calling the correct methods
      handlerMock.Protected().Verify(
            "SendAsync",
          Times.Exactly(3), // two to /token one to /revoke
          ItExpr.Is<HttpRequestMessage>(arg =>

            arg.Headers.Authorization.ToString() == $"Basic {accessToken}"
           ),
            ItExpr.IsAny<CancellationToken>()
         );
    }

    /// <summary>
    /// TPaaSClient should be able to refresh an expired token.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task TestTPaaSClientTokenThrowsExceptionBadRevokeResponse()
    {
      var accessToken = "token";
      var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);

      handlerMock
         .Protected()
         // Setup the protected method to mock
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
             StatusCode = HttpStatusCode.Unauthorized,
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

      var tpaasClient = new TPaaSClient(httpClient, new Mock<ILogger<TPaaSClient>>().Object);
      var clientState = new Mock<ITPaaSClientState>().SetupAllProperties();
      clientState.Object.TPaaSTokenExpiry = DateTime.MinValue;
      clientState.Object.TPaaSToken = accessToken;

      await tpaasClient.setState(clientState.Object);


      Func<Task> act = async () => { await tpaasClient.GetBearerTokenAsync(); };
      act.Should().Throw<TPaaSAuthenticationException>().WithMessage("Error revoking access token*");

      //Check that the client calling the correct methods
      handlerMock.Protected().Verify(
            "SendAsync",
          Times.Once(), // two to /token one to /revoke
          ItExpr.Is<HttpRequestMessage>(arg =>

            arg.Headers.Authorization.ToString() == $"Basic {accessToken}"
           ),
            ItExpr.IsAny<CancellationToken>()
         );
    }

    /// <summary>
    /// TPaaSClient should be able to refresh an expired token.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task TestTPaaSClientTokenThrowsExceptionBadTokenResponse()
    {
      var accessToken = "token";
      var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);

      handlerMock
         .Protected()
         // Setup the protected method to mock
         .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(request =>
            request.RequestUri.ToString().Contains("/token") &&
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
             RequestMessage = req
           };
         })
         .Verifiable();

      handlerMock
       .Protected()
       // Setup the protected method to mock calls to /revoke
       .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.Is<HttpRequestMessage>(request =>
          request.RequestUri.ToString().Contains("/revoke") &&
          request.Headers.Authorization.ToString() == $"Basic {accessToken}"
          ),
          ItExpr.IsAny<CancellationToken>()
       )
       .ReturnsAsync(
          new HttpResponseMessage()
          {
            StatusCode = HttpStatusCode.OK,
          }
      )
      .Verifiable();

      var authHandler = new TPaaSApplicationCredentialsRequestHandler(handlerMock.Object);
      authHandler.SetTPaasToken(accessToken);

      // use real http client with mocked handler here
      var httpClient = new HttpClient(authHandler)
      {
        BaseAddress = new Uri("http://nowhere.specific/"),
      };

      var tpaasClient = new TPaaSClient(httpClient, new Mock<ILogger<TPaaSClient>>().Object);
      var clientState = new Mock<ITPaaSClientState>().SetupAllProperties();
      clientState.Object.TPaaSTokenExpiry = DateTime.MinValue;
      clientState.Object.TPaaSToken = accessToken;

      await tpaasClient.setState(clientState.Object);


      Func<Task> act = async () => { await tpaasClient.GetBearerTokenAsync(); };
      act.Should().Throw<TPaaSAuthenticationException>().WithMessage("No content response from TPaaS while getting token");

      //Check that the client calling the correct methods
      handlerMock.Protected().Verify(
            "SendAsync",
          Times.Exactly(2), // two to /token one to /revoke
          ItExpr.Is<HttpRequestMessage>(arg =>

            arg.Headers.Authorization.ToString() == $"Basic {accessToken}"
           ),
            ItExpr.IsAny<CancellationToken>()
         );
    }


    /// <summary>
    /// TPaaSClient should be able to refresh an expired token.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task TestTPaaSClientTokenThrowsExceptionUnAuthorisedTokenResponse()
    {
      var accessToken = "token";
      var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);

      handlerMock
         .Protected()
         // Setup the protected method to mock
         .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(request =>
            request.RequestUri.ToString().Contains("/token") &&
            request.Headers.Authorization.ToString() == $"Basic {accessToken}"
            ),
            ItExpr.IsAny<CancellationToken>()
         )
         // prepare the expected response of the mocked http call
         .ReturnsAsync((HttpRequestMessage req, CancellationToken token) =>
         {
           return new HttpResponseMessage()
           {
             StatusCode = HttpStatusCode.Unauthorized,
             RequestMessage = req
           };
         })
         .Verifiable();

      handlerMock
       .Protected()
       // Setup the protected method to mock calls to /revoke
       .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.Is<HttpRequestMessage>(request =>
          request.RequestUri.ToString().Contains("/revoke") &&
          request.Headers.Authorization.ToString() == $"Basic {accessToken}"
          ),
          ItExpr.IsAny<CancellationToken>()
       )
       .ReturnsAsync(
          new HttpResponseMessage()
          {
            StatusCode = HttpStatusCode.OK,
          }
      )
      .Verifiable();

      var authHandler = new TPaaSApplicationCredentialsRequestHandler(handlerMock.Object);
      authHandler.SetTPaasToken(accessToken);

      // use real http client with mocked handler here
      var httpClient = new HttpClient(authHandler)
      {
        BaseAddress = new Uri("http://nowhere.specific/"),
      };

      var tpaasClient = new TPaaSClient(httpClient, new Mock<ILogger<TPaaSClient>>().Object);
      var clientState = new Mock<ITPaaSClientState>().SetupAllProperties();
      clientState.Object.TPaaSTokenExpiry = DateTime.MinValue;
      clientState.Object.TPaaSToken = accessToken;

      await tpaasClient.setState(clientState.Object);


      Func<Task> act = async () => { await tpaasClient.GetBearerTokenAsync(); };
      act.Should().Throw<TPaaSAuthenticationException>().WithMessage("Could not authenticate with TPaaS*");

      //Check that the client calling the correct methods
      handlerMock.Protected().Verify(
            "SendAsync",
          Times.Exactly(2), // two to /token one to /revoke
          ItExpr.Is<HttpRequestMessage>(arg =>

            arg.Headers.Authorization.ToString() == $"Basic {accessToken}"
           ),
            ItExpr.IsAny<CancellationToken>()
         );
    }

  }
}
