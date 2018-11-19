using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using VSS.TRex.HttpClients.RequestHandlers;
using VSS.TRrex.HttpClients.Abstractions;
using Xunit;


namespace VSS.TRex.HttpClients.Tests
{
  public class TPaaSAuthenticatedRequestHandlerTests
  {


    [Fact]
    public async Task TestTPaaSAuthHeaderIsAdded_Get()
    {
      var mockBearerToken = "Bearer token";
      var mockTPaaSClient = new Mock<ITPaaSClient>();

      mockTPaaSClient.Setup(client => client.GetBearerTokenAsync()).Returns(Task.FromResult(mockBearerToken));

      var services = new ServiceCollection();
      services.AddSingleton(mockTPaaSClient.Object);
      DI.DIContext.Inject(services.BuildServiceProvider());

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
