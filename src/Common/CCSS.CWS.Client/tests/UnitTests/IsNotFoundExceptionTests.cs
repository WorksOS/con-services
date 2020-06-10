using System.Net.Http;
using Xunit;

namespace CCSS.CWS.Client.UnitTests
{
  public class IsNotFoundExceptionTests
  {
    [Fact]
    public void IsNotFoundException_Status404()
    {
      var exMessage = "NotFound {\"status\": 404,\"code\": 11012,\"moreInfo\": \"Please provide this id to support, while contacting, TraceId 5edf0d36384f82d77d8e1951f7edb1d9\",\"message\": \"Config File not found for given project Id\",\"timestamp\": \"2020-06-09T04:16:54.582+0000\"}";
      var ex = new HttpRequestException(exMessage);
      var result = ex.IsNotFoundException();
      Assert.True(result);
    }

    [Fact]
    public void IsNotFoundException_Contains404InMessage()
    {
      var exMessage = "BadRequest { \"status\":400,\"code\":9021,\"message\":\"Invalid project id\",\"moreInfo\":\"Please provide this id to support, while contacting, TraceId 5ecdaaf08b82c37783a01540d374048c\",\"timestamp\":1590536944256}";
      var ex = new HttpRequestException(exMessage);
      var result = ex.IsNotFoundException();
      Assert.False(result);
    }

    [Fact]
    public void IsNotFoundException_Not404()
    {
      var exMessage = "{\"status\": 401,\"code\": 9008,\"message\": \"Unauthorized\",\"moreInfo\": \"Please provide this id to support, while contacting, TraceId 5edf1055c0abaa1ca3269838149fe852\",\"timestamp\": 1591677013115}";
      //var exMessage = "BadRequest {\"status\":400,\"code\":9056,\"message\":\"Bad request\",\"moreInfo\":\"Please provide this id to support, while contacting, TraceId 5edf0abbaa596f2bc2288dfebbbe0b0b\",\"timestamp\":1591675579758}";
      var ex = new HttpRequestException(exMessage);
      var result = ex.IsNotFoundException();
      Assert.False(result);
    }
  }
}
