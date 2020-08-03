using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Moq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Http;
using Xunit;

namespace VSS.MasterData.Proxies.UnitTests
{
  public class MockStartup
  {
    public void ConfigureServices(IServiceCollection services)
    { }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    { }
  }

  public class GracefulWebRequestTests : IClassFixture<MemoryCacheTestsFixture>
  {
    private readonly IServiceProvider _serviceProvider;
    private Func<HttpContext, Task<bool>> _testFunc;

    public class TestClass
    {
      public string String { get; set; }
      public decimal Number { get; set; }
      public byte[] Binary { get; set; }
      public string Unicode { get; set; }

      public override bool Equals(object obj) => Equals(obj as TestClass);

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
          var hashCode = String != null ? String.GetHashCode() : 0;
          hashCode = (hashCode * 397) ^ Number.GetHashCode();
          hashCode = (hashCode * 397) ^ (Binary?.GetHashCode() ?? 0);
          hashCode = (hashCode * 397) ^ (Unicode?.GetHashCode() ?? 0);
          return hashCode;
        }
      }
    }

    public GracefulWebRequestTests(MemoryCacheTestsFixture testFixture)
    {
      var host = new HostBuilder()
      .ConfigureWebHost(webBuilder =>
      {
        webBuilder
        .UseTestServer()
        .ConfigureServices(services =>
        { })
        .Configure(app =>
        {
          app.Run(async context => await _testFunc(context));
        });
      }).Start();

      var mockFactory = new Mock<IHttpClientFactory>();
      mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(host.GetTestClient());

      _serviceProvider = testFixture.serviceCollection
        .AddSingleton<IHttpClientFactory>(mockFactory.Object)
        .BuildServiceProvider();
    }

    /// <summary>
    /// Helper method to calculate MD5 sum of a stream
    /// </summary>
    private static string CalculateMD5(Stream stream)
    {
      using var md5 = MD5.Create();

      stream.Position = 0;
      var md5Hash = md5.ComputeHash(stream);

      return BitConverter.ToString(md5Hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Execute a post request, and validate the contents of the body
    /// </summary>
    private async Task ExecutePostRequest(Stream data, string contentType, Func<Stream, Task<bool>> validateBodyFunc)
    {
      var gracefulWebRequest = ActivatorUtilities.CreateInstance(_serviceProvider, typeof(GracefulWebRequest)) as GracefulWebRequest;
      Assert.NotNull(gracefulWebRequest);

      var requestPassed = false;

      _testFunc = async httpContext =>
      {
        using (var ms = new MemoryStream())
        {
          await httpContext.Request.Body.CopyToAsync(ms);
          requestPassed = await validateBodyFunc(ms);
        }

        // Validate the content type passed, this should not be modified
        if (!string.Equals(httpContext.Request.Headers[HeaderNames.ContentType], contentType, StringComparison.CurrentCultureIgnoreCase))
        {
          Console.WriteLine($"Expected Content Type : '{contentType}' but got '{httpContext.Request.Headers[HeaderNames.ContentType]}' - Failed!");
          return false;
        }
        return requestPassed;
      };

      var headers = new HeaderDictionary
      {
        [HeaderNames.ContentType] = contentType
      };

      await gracefulWebRequest.ExecuteRequest("http://localhost:51000", data, headers, HttpMethod.Post, null, 0, true);
    }

    [Fact]
    public async Task TestPostBinaryDataWithNoContentType()
    {
      const string BINARY_TEST_FILENAME = "TestFiles/TestBinary.ttm";
      const string CORRECT_MD5 = "36dd727b3ac476d39ee98daf465a0658"; // calculated manually
      const string EXPECTED_CONTENT_TYPE = ContentTypeConstants.ApplicationOctetStream;

      Assert.True(File.Exists(BINARY_TEST_FILENAME), $"Test file '{BINARY_TEST_FILENAME}' doesn't exist");

      var memoryStream = new MemoryStream(File.ReadAllBytes(BINARY_TEST_FILENAME));
      var md5 = CalculateMD5(memoryStream);
      Console.WriteLine($"File MD5: {md5}");
      Assert.True(string.Compare(md5, CORRECT_MD5, StringComparison.InvariantCultureIgnoreCase) == 0, "Invalid test data file");

      var requestPassed = false;

      var validatePostedData = new Func<Stream, Task<bool>>((stream) =>
      {
        var requestMd5 = CalculateMD5(stream);
        Console.WriteLine($"Request md5, Got '{requestMd5}', expected '{CORRECT_MD5}'. " +
                          $"Got length : {stream.Length}, expected length: {memoryStream.Length}");

        requestPassed = string.Compare(requestMd5, CORRECT_MD5, StringComparison.InvariantCultureIgnoreCase) == 0;

        return Task.FromResult(requestPassed);
      });

      await ExecutePostRequest(memoryStream, EXPECTED_CONTENT_TYPE, validatePostedData);

      Assert.True(requestPassed);
    }

    [Fact]
    public async Task TestGetJsonData()
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

      var validateData = new Func<Stream, Task<bool>>(async (stream) =>
      {
        stream.Position = 0;

        using var reader = new StreamReader(stream, Encoding.UTF8);
        var data = await reader.ReadToEndAsync();
        Console.WriteLine($"Got Response Model JSON: {data}");
        resultModel = JsonConvert.DeserializeObject<TestClass>(data);

        return true;
      });

      await ExecutePostRequest(jsonTestMemoryStream, ContentTypeConstants.ApplicationJson, validateData);

      Assert.NotNull(resultModel);
      Assert.Equal(jsonTestModel, JsonConvert.SerializeObject(resultModel));
      Assert.True(testModel.Equals(resultModel));
    }
  }
}
