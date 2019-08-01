using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Newtonsoft.Json;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharp.Headers;
using uhttpsharp.Listeners;
using uhttpsharp.Logging;
using uhttpsharp.RequestProviders;
using VSS.Common.Abstractions.Http;
using Xunit;

namespace VSS.MasterData.Proxies.UnitTests
{
  public class GracefulWebRequestTests : IClassFixture<MemoryCacheTestsFixture>
  {
    public class EmptyHandler : IHttpRequestHandler
    {
      public Task Handle(IHttpContext context, Func<Task> next)
      {
        context.Response = HttpResponse.CreateWithMessage(HttpResponseCode.Ok, "Empty Response", context.Request.Headers.KeepAliveConnection());

        return Task.Factory.GetCompleted();
      }
    }

    public class TestClass
    {
      public string String { get; set; }
      public decimal Number { get; set; }
      public byte[] Binary { get; set; }
      public string Unicode { get; set; }

      public override bool Equals(object obj)
      {
        return Equals(obj as TestClass);
      }

      protected bool Equals(TestClass other)
      {
        if (other == null)
          return false;
        var stringEquals = string.Equals(String, other.String);
        var numberEquals = Number == other.Number;
        var binaryEquals = !Binary.Where((t, i) => t != other.Binary[i]).Any();
        var unicodeEquals = string.Equals(Unicode, other.Unicode);

        return stringEquals && numberEquals && binaryEquals && unicodeEquals;
      }

      public override int GetHashCode()
      {
        unchecked
        {
          var hashCode = (String != null ? String.GetHashCode() : 0);
          hashCode = (hashCode * 397) ^ Number.GetHashCode();
          hashCode = (hashCode * 397) ^ (Binary != null ? Binary.GetHashCode() : 0);
          hashCode = (hashCode * 397) ^ (Unicode != null ? Unicode.GetHashCode() : 0);
          return hashCode;
        }
      }
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly Random _random = new Random();

    public GracefulWebRequestTests(MemoryCacheTestsFixture testFixture)
    {
      _serviceProvider = testFixture.serviceProvider;
    }

    private int GetPortForWebServer()
    {
      const int minPort = 50000;
      const int maxPort = 51000;
      const int maxRetries = 10;

      var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
      var ipEndPoints = ipProperties.GetActiveTcpListeners();

      var retry = 0;
      while (retry++ < maxRetries)
      {
        var port = _random.Next(minPort, maxPort);
        if (ipEndPoints.Any(endPoint => endPoint.Port == port))
          continue;

        return port;
      }

      throw new TestCanceledException($"No available ports for WebServer after {maxRetries} - Failing test");
    }

    /// <summary>
    /// Creates an empty Test HTTP Server, than response with Ok on a request to /
    /// And takes a validate request function which is called for each request
    /// </summary>
    private HttpServer CreateServer(int port, Func<IHttpContext, bool> validateRequestFunc)
    {
      // The default implementation of the LogProviders depend on various console libraries which are not available in unit tests
      // And we don't really care about the internal logging of the server...
      LogProvider.LogProviderResolvers.Clear();
      var httpServer = new HttpServer(new HttpRequestProvider());
      httpServer.Use(new TcpListenerAdapter(new TcpListener(IPAddress.Loopback, port)));

      // Request handling : 
      httpServer.Use((context, next) =>
      {
        Assert.True(validateRequestFunc(context));
        return next();
      });
      httpServer.Use(new HttpRouter().With(string.Empty, new EmptyHandler())); // make sure we return an OK Response
      httpServer.Start();
      return httpServer;
    }

    /// <summary>
    /// Helper method to calculate MD5 sum of a stream
    /// </summary>
    private static string CalculateMD5(Stream stream)
    {
      using (var md5 = MD5.Create())
      {
        stream.Seek(0, SeekOrigin.Begin);
        var hash = md5.ComputeHash(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
      }
    }

    /// <summary>
    /// Execute a post request, and validate the contents of the body
    /// </summary>
    private bool ExecutePostRequest(MemoryStream data, string contentType, Func<MemoryStream, bool> validateBodyFunc)
    {
      var port = GetPortForWebServer();

      var gracefulWebRequest = ActivatorUtilities.CreateInstance(_serviceProvider, typeof(GracefulWebRequest)) as GracefulWebRequest;
      Assert.NotNull(gracefulWebRequest);

      var requestPassed = false;

      bool ValidatePostedData(IHttpContext c)
      {

        var requestStream = new MemoryStream(c.Request.Post.Raw);
        requestPassed = validateBodyFunc(requestStream);
        // Validate the content type passed, this should not be modified
        if (string.Compare(c.Request.Headers.GetByName(HeaderNames.ContentType), contentType,
              StringComparison.CurrentCultureIgnoreCase) != 0)
        {
          Console.WriteLine($"Execpted Content Type : '{contentType}' but got '{c.Request.Headers.GetByName(HeaderNames.ContentType)}' - Failed!");
          return false;
        }
        return requestPassed;
      }

      using (CreateServer(port, ValidatePostedData))
      {
        var headers = new Dictionary<string, string>
        {
          [HeaderNames.ContentType] = contentType
        };
        var url = $"http://localhost:{port}";
        data.Seek(0, SeekOrigin.Begin);
        gracefulWebRequest.ExecuteRequest(url, data, headers, HttpMethod.Post, null, 0, true).Wait();
      }

      return requestPassed;
    }

    [Fact]
    public void TestPostBinaryDataWithNoContentType()
    {
      const string BinaryTestFilename = "TestFiles/TestBinary.ttm";

      const string correctMd5 = "36dd727b3ac476d39ee98daf465a0658"; // calculated manually
      const string expectedContentType = ContentTypeConstants.ApplicationOctetStream;
      Assert.True(File.Exists(BinaryTestFilename), $"Test file '{BinaryTestFilename}' doesn't exist");

      var memoryStream = new MemoryStream(File.ReadAllBytes(BinaryTestFilename));
      var md5 = CalculateMD5(memoryStream);
      Console.WriteLine($"File MD5: {md5}");
      Assert.True(string.Compare(md5, correctMd5, StringComparison.InvariantCultureIgnoreCase) == 0, "Invalid test data file");

      var requestPassed = false;
      var validatePostedData = new Func<MemoryStream, bool>((stream) =>
      {
        var requestMd5 = CalculateMD5(stream);
        Console.WriteLine($"Request md5, Got '{requestMd5}', expected '{correctMd5}'. " +
                          $"Got length : {stream.Length}, expected length: {memoryStream.Length}");

        requestPassed = string.Compare(requestMd5, correctMd5, StringComparison.InvariantCultureIgnoreCase) == 0;

        return requestPassed;
      });

      Assert.True(ExecutePostRequest(memoryStream, expectedContentType, validatePostedData));

      Assert.True(requestPassed);
    }

    [Fact]
    public void TestGetJsonData()
    {
      var testModel = new TestClass()
      {
        String = "Test Data",
        Number = 2.05e5m,
        Binary = new byte[]
        {
          0xfd, 0xfe, 0xed, 0x2c, 0x9c, 0x33, 0xac, 0x61, 0xb0, 0x9c, 0x5f, 0x35, 0xff, 0x2e, 0xab, 0xcc, 0x4a, 0xd7,
          0xbf, 0xa6, 0x03, 0x83, 0x2d, 0x63, 0xe5, 0x5f, 0xee, 0x85, 0x19, 0x8f, 0xcf, 0x26, 0xd9, 0xec, 0x7a, 0x27,
          0x8a, 0x70, 0xc0, 0xa6, 0xde, 0x57, 0x1b, 0xd0, 0x81, 0xbf, 0x1f, 0xee, 0xaf, 0xd2
        },
        Unicode = @"Ω≈ç√∫˜µ≤≥÷" +
                  @"åß∂ƒ©˙∆˚¬…æ" +
                  @"œ∑´®†¥¨ˆøπ“‘" +
                  @"¡™£¢∞§¶•ªº–≠" +
                  @"¸˛Ç◊ı˜Â¯˘¿" +
                  @"ÅÍÎÏ˝ÓÔÒÚÆ☃" +
                  @"Œ„´‰ˇÁ¨ˆØ∏”’" +
                  @"`⁄€‹›ﬁﬂ‡°·‚—±" +
                  @"⅛⅜⅝⅞" +
                  @"ЁЂЃЄЅІЇЈЉЊЋЌЍЎЏАБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзийклмнопрстуфхцчшщъыьэюя" +
                  @"٠١٢٣٤٥٦٧٨٩" +
                  @"⁰⁴⁵" +
                  @"₀₁₂" +
                  @"⁰⁴⁵₀₁₂" +
                  @"¯\_(ツ)_/¯" +
                  @"❤️ 💔 💌 💕 💞 💓 💗 💖 💘 💝 💟 💜 💛 💚 💙"

      };

      TestClass resultModel = null;

      var jsonTestModel = JsonConvert.SerializeObject(testModel);
      Console.WriteLine($"Sending JSON: {jsonTestModel}");
      var jsonTestMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonTestModel));

      var validateData = new Func<MemoryStream, bool>((stream) =>
      {
        var data = Encoding.UTF8.GetString(stream.ToArray());
        Console.WriteLine($"Got Response Model JSON: {data}");
        resultModel = JsonConvert.DeserializeObject<TestClass>(data);

        return true;
      });

      Assert.True(ExecutePostRequest(jsonTestMemoryStream, ContentTypeConstants.ApplicationJson, validateData));

      Assert.NotNull(resultModel);
      Assert.Equal(jsonTestModel, JsonConvert.SerializeObject(resultModel));
      Assert.True(testModel.Equals(resultModel));
    }

  }
}
