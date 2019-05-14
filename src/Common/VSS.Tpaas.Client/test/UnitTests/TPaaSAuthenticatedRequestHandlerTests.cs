using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using VSS.Tpaas.Client.Abstractions;
using VSS.Tpaas.Client.Models;
using VSS.Tpaas.Client.RequestHandlers;
using Xunit;

namespace VSS.Tpaas.Client.UnitTests
{
  public class TPaaSAuthenticatedRequestHandlerTests
  {


    [Fact]
    public async Task TestTPaaSAuthHeaderIsAdded_Get()
    {
      var mockBearerToken = "Bearer token";
      var mockTPaaSClient = new Mock<ITPaaSClient>();

      mockTPaaSClient.Setup(client => client.GetBearerTokenAsync()).Returns(Task.FromResult(mockBearerToken));

      var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
      handlerMock
         .Protected()
         // Setup the PROTECTED method to mock
         .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
         )
         // prepare the expected response of the mocked http call
         .ReturnsAsync((HttpRequestMessage req, CancellationToken token) =>
         {
           return new HttpResponseMessage()
           {
             StatusCode = HttpStatusCode.OK,
             Content = new StringContent(string.Empty),
             RequestMessage = req
           };
         })
         .Verifiable();

      var authHandler = new TPaaSAuthenticatedRequestHandler(handlerMock.Object);
      authHandler.TPaaSClient = mockTPaaSClient.Object;

      // use real http client with mocked handler here
      var httpClient = new HttpClient(authHandler)
      {
        BaseAddress = new Uri("http://nowhere.specific/")
      };

      var result = await httpClient.GetAsync("");
 
      result.Should().NotBeNull();

      //Check that the middleware is adding the header
      handlerMock.Protected().Verify(
          "SendAsync",
          Times.Once(), // we expected a single request
          ItExpr.Is<HttpRequestMessage>(request =>
            request.Headers.Authorization.ToString().Equals(mockBearerToken)
         ),
          ItExpr.IsAny<CancellationToken>()
       );
    }


    [Fact]
    public async Task TestCorrectExceptionIfClientIsNotDId()
    {
      var mockBearerToken = "Bearer token";
      var mockTPaaSClient = new Mock<ITPaaSClient>();

      mockTPaaSClient.Setup(client => client.GetBearerTokenAsync()).Returns(Task.FromResult(mockBearerToken));

      var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
      handlerMock
         .Protected()
         // Setup the PROTECTED method to mock
         .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
         )
         // prepare the expected response of the mocked http call
         .ReturnsAsync((HttpRequestMessage req, CancellationToken token) =>
         {
           return new HttpResponseMessage()
           {
             StatusCode = HttpStatusCode.OK,
             Content = new StringContent(string.Empty),
             RequestMessage = req
           };
         })
         .Verifiable();

      var authHandler = new TPaaSAuthenticatedRequestHandler(handlerMock.Object);

      // use real http client with mocked handler here
      var httpClient = new HttpClient(authHandler)
      {
        BaseAddress = new Uri("http://nowhere.specific/")
      };

      Func<Task> result = async () => { await httpClient.GetAsync(""); };
      result.Should().Throw<TPaaSAuthenticatedRequestHandlerException>()
        .WithMessage("Bearer could not be obtained, have you DI'd the TPaaSAppCreds Client?");
      

      //Check that the middleware is adding the header
      handlerMock.Protected().Verify(
          "SendAsync",
          Times.Never(), // we expected a single request
          ItExpr.Is<HttpRequestMessage>(request =>
            request.Headers.Authorization.ToString().Equals(mockBearerToken)
         ),
          ItExpr.IsAny<CancellationToken>()
       );
    }

  }
}
