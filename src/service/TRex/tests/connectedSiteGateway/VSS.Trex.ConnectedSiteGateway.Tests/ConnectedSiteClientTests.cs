using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using VSS.TRex.ConnectedSite.Gateway.WebApi;
using VSS.TRex.ConnectedSite.Gateway.WebApi.Models;
using VSS.TRex.Types;
using Xunit;

namespace VSS.Trex.ConnectedSiteGateway.Tests
{
  public class ConnectedSiteClientTests
  {
    [Fact]
    public async Task Test_Good_L1_Message()
    {
      // Test constants
      var expectedUri = new Uri("http://nowhere.specific/positions/in/v1/GCS900-1");
      var messageTime = DateTime.Parse("2018-10-16T16:54:12.9933999+13:00");
      var expectedRequestMessage = "{\"ts\":\"2018-10-16T16:54:12.9933999+13:00\",\"lat\":-2.0,\"lon\":3.0,\"h\":1.0}";

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

      // use real http client with mocked handler here
      var httpClient = new HttpClient(handlerMock.Object)
      {
        BaseAddress = new Uri("http://nowhere.specific/"),
      };

      var l1Message = new L1ConnectedSiteMessage
      {
        Height = 1,
        Lattitude = -2,
        Longitude = 3,
        Timestamp = messageTime,
        HardwareID = "1"
      };

      var client = new ConnectedSiteClient(httpClient, new Mock<ILogger<ConnectedSiteClient>>().Object);

      var result = await client.PostMessage(l1Message);

      result.Should().NotBeNull();
      result.StatusCode.Should().Be(HttpStatusCode.OK);
      handlerMock.Protected().Verify(
         "SendAsync",
         Times.Once(), // we expected a single request
         ItExpr.IsAny<HttpRequestMessage>(),
         ItExpr.IsAny<CancellationToken>()
      );

      result.RequestMessage.Content.ReadAsStringAsync().Result.Should().Be(expectedRequestMessage);
      result.RequestMessage.RequestUri.Should().Be(expectedUri);
    }


    [Fact]
    public async Task Test_Good_L2_Message()
    {
      // Test constants
      var expectedUri = new Uri("http://nowhere.specific/status/in/v1/GCS900-1");
      var messageTime = DateTime.Parse("2018-10-16T16:54:12.9933999+13:00");
      var expectedRequestMessage =
        "{\"timestamp\":\"2018-10-16T16:54:12.9933999+13:00\",\"designName\":\"Highway to hell\",\"assetType\":\"Dozer\",\"appVersion\":\"666a\",\"appName\":\"GCS900\",\"assetNickname\":\"Little Nicky\",\"lat\":-2.0,\"lon\":3.0,\"h\":1.0}";

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

      // use real http client with mocked handler here
      var httpClient = new HttpClient(handlerMock.Object)
      {
        BaseAddress = new Uri("http://nowhere.specific/"),
      };

      var l2Message = new L2ConnectedSiteMessage
      {
        Height = 1,
        Lattitude = -2,
        Longitude = 3,
        Timestamp = messageTime,
        HardwareID = "1",
        DesignName = "Highway to hell",
        AppVersion = "666a",
        AssetNickname = "Little Nicky",
        AssetType = ((MachineType)0x17).ToString()
       };

      var client = new ConnectedSiteClient(httpClient, new Mock<ILogger<ConnectedSiteClient>>().Object);

      var result = await client.PostMessage(l2Message);

      result.Should().NotBeNull();
      result.StatusCode.Should().Be(HttpStatusCode.OK);
      handlerMock.Protected().Verify(
         "SendAsync",
         Times.Once(), // we expected a single request
         ItExpr.IsAny<HttpRequestMessage>(),
         ItExpr.IsAny<CancellationToken>()
      );

      result.RequestMessage.Content.ReadAsStringAsync().Result.Should().Be(expectedRequestMessage);
      result.RequestMessage.RequestUri.Should().Be(expectedUri);
    }


    [Fact]
    public async Task Test_Server_Exception_Response()
    {
      var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
      handlerMock
         .Protected()
         // Setup the PROTECTED method to mock
         .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
         )
         .Throws(new HttpRequestException("OHHH NOOO"))         
         .Verifiable();

      // use real http client with mocked handler here
      var httpClient = new HttpClient(handlerMock.Object)
      {
        BaseAddress = new Uri("http://nowhere.specific/"),
      };

      var l2Message = new L2ConnectedSiteMessage
      {
        Height = 1,
        Lattitude = -2,
        Longitude = 3,
        Timestamp = DateTime.Now,
        HardwareID = "1",
        DesignName = "Highway to hell",
        AssetType = "ICBM",
        AppVersion = "666a",
        AssetNickname = "Little Nicky",
      };

      var logger = new Mock<ILogger<ConnectedSiteClient>>();
      var client = new ConnectedSiteClient(httpClient, logger.Object);

      Func<Task> act = async () => { await client.PostMessage(l2Message); };
      act.Should().Throw<HttpRequestException>().WithMessage("OHHH NOOO*");

      handlerMock.Protected().Verify(
         "SendAsync",
         Times.Once(), // we expected a single request
         ItExpr.IsAny<HttpRequestMessage>(),
         ItExpr.IsAny<CancellationToken>()
      );
    }
  }
}
