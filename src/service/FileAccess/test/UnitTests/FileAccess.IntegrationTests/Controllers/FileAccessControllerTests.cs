using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.Productivity3D.FileAccess.WebAPI.Models.Models;
using Xunit;

namespace FileAccess.IntegrationTests.Controllers
{
  public class FileAccessControllerTests : IClassFixture<TestClientProviderFixture>
  {
    private readonly HttpClient client;

    public FileAccessControllerTests(TestClientProviderFixture testFixture)
    {
      client = testFixture.Client;
    }

    private async Task<HttpResponseMessage> PostAsync(string uri, object body) =>
      await client.PostAsync(uri,
                             new StringContent(
                               JsonConvert.SerializeObject(body),
                               Encoding.UTF8, "application/json"));

    [Fact]
    public async Task ShouldDownloadFile()
    {
      var body = FileDescriptor.CreateFileDescriptor("5u8472cda0-9f59-41c9-a5e2-e19f922f91d8", "/77561/1158", "Large Sites Road - Trimble Road.ttm");
      var response = await PostAsync("http://localhost/api/v1/rawfiles", body);

      response.EnsureSuccessStatusCode();
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      Assert.IsType<StreamContent>(response.Content);
    }

    [Fact]
    public async Task ShouldReturnFailedDownloadResponse()
    {
      var body = FileDescriptor.CreateFileDescriptor("u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01", "/77561/1158", "IDontExist.ttm");
      var response = await PostAsync("http://localhost/api/v1/rawfiles", body);

      response.EnsureSuccessStatusCode();
      Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
  }
}
