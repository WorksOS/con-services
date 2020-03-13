using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using VSS.MasterData.Proxies.Interfaces;
using FluentAssertions.Json;

namespace CCSS.CWS.Client.UnitTests.Mocked
{
  public class MockUtilities
  {
    /// <summary>
    /// Mocks the IWebRequest class to test the proxy class in question. This will ensure the proxy class calls the correct endpoint
    /// And the request object converts to the correct JSON
    /// </summary>
    public static void TestRequestSendsCorrectJson<TResponse>(string what, Mock<IWebRequest> mockWebRequest,
      string expectedJson, string url, HttpMethod method, TResponse response, Func<Task<bool>> testExecution)
    {
      SetupMockRequest(mockWebRequest,
        url,
        () => response,
        validateUrlAction: (requestUrl) => Assert.AreEqual(requestUrl, url),
        validateStreamAction: (requestStream) => ValidateRequestStream(requestStream, expectedJson),
        validateHttpMethodAction: (requestMethod) => Assert.AreEqual(requestMethod, method, $"{what} should be {method} but it is {requestMethod}")
      );

      var result = testExecution.Invoke().Result;
      Assert.IsTrue(result);
    }

    public static void TestRequestSendsCorrectJson(string what, Mock<IWebRequest> mockWebRequest,
      string expectedJson, string url, HttpMethod method, Func<Task<bool>> testExecution)
    {
      SetupMockRequest(mockWebRequest,
        url,
        validateUrlAction: (requestUrl) => Assert.AreEqual(requestUrl, url),
        validateStreamAction: (requestStream) => ValidateRequestStream(requestStream, expectedJson),
        validateHttpMethodAction: (requestMethod) => Assert.AreEqual(requestMethod, method, $"{what} should be {method} but it is {requestMethod}")
      );

      var result = testExecution.Invoke().Result;
      Assert.IsTrue(result);
    }

    private static void ValidateRequestStream(Stream requestStream, string expectedJson)
    {
      if (!string.IsNullOrEmpty(expectedJson))
      {
        requestStream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(requestStream, Encoding.UTF8, false, 1024, true);
        var text = reader.ReadToEnd();

        var actual = JToken.Parse(text);
        actual.Should().BeEquivalentTo(JToken.Parse(expectedJson));
      }
    }

    private static void SetupMockRequest<TResponse>(Mock<IWebRequest> mockWebRequest,
      string endpointAddress,
      Func<TResponse> responseFunc,
      Action<string> validateUrlAction = null,
      Action<Stream> validateStreamAction = null,
      Action<HttpMethod> validateHttpMethodAction = null)
    {
      mockWebRequest.Setup(s => s.ExecuteRequest<TResponse>(It.Is<string>(v => string.Compare(v, endpointAddress, StringComparison.Ordinal) == 0),
          It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<HttpMethod>(),
          It.IsAny<int?>(),
          It.IsAny<int>(),
          It.IsAny<bool>()))
        .Callback<string, Stream, IDictionary<string, string>, HttpMethod, int?, int, bool>((url, stream, _, method, __, ___, ____) =>
        Validate(url, stream, method, validateUrlAction, validateStreamAction, validateHttpMethodAction))
        .Returns(Task.FromResult(responseFunc()));
    }

    private static void SetupMockRequest(Mock<IWebRequest> mockWebRequest,
      string endpointAddress,
      Action<string> validateUrlAction = null,
      Action<Stream> validateStreamAction = null,
      Action<HttpMethod> validateHttpMethodAction = null)
    {
      mockWebRequest.Setup(s => s.ExecuteRequest(It.Is<string>(v => string.Compare(v, endpointAddress, StringComparison.Ordinal) == 0),
          It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<HttpMethod>(),
          It.IsAny<int?>(),
          It.IsAny<int>(),
          It.IsAny<bool>()))
        .Callback<string, Stream, IDictionary<string, string>, HttpMethod, int?, int, bool>((url, stream, _, method, __, ___, ____) =>
        Validate(url, stream, method, validateUrlAction, validateStreamAction, validateHttpMethodAction))
        .Returns(Task.CompletedTask);
    }

    private static void Validate(string url, Stream stream, HttpMethod method,
      Action<string> validateUrlAction = null,
      Action<Stream> validateStreamAction = null,
      Action<HttpMethod> validateHttpMethodAction = null)
    {
      validateUrlAction?.Invoke(url);

      validateHttpMethodAction?.Invoke(method);

      validateStreamAction?.Invoke(stream);
    }
  }
}
