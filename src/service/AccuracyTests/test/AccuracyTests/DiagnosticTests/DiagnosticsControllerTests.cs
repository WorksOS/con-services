using System.Net.Http;
using System.Threading.Tasks;
using CCSS.IntegrationTests.Utils;
using CCSS.IntegrationTests.Utils.Extensions;
using CCSS.IntegrationTests.Utils.Types;
using FluentAssertions;
using Xunit;

namespace AccuracyTests
{
  public class SimplePingResult
  {
    public int UserId;
    public int Id;
    public string Title;
    public bool Completed;
  }

  public class DiagnosticsControllerTests : IClassFixture<TestClientProviderFixture>
  {
    private readonly TestClientProviderFixture _fixture;
    private readonly IRestClient _restClient;

    public DiagnosticsControllerTests(TestClientProviderFixture testFixture)
    {
      _restClient = testFixture.RestClient;
      _fixture = testFixture;
    }

    // Demo simple GET request and deserialize back to a known model object.
    [Fact]
    public async Task Simple_ping_test()
    {
      var response = await _restClient
        .SendAsync("https://jsonplaceholder.typicode.com/todos/1", HttpMethod.Get, acceptHeader: MediaTypes.JSON);

      response.IsSuccessStatusCode.Should().BeTrue();
      var resultObj = await response.ConvertToType<SimplePingResult>();

      resultObj.Id.Should().Be(1);
      resultObj.Completed.Should().BeFalse();
    }

    // Demo using DataGenerator common method (Peferrered method).
    [Fact]
    public async Task Simple_ping_test_2()
    {
      // Using TestDataGenerator methods the 'test' becomes a simple list of steps 
      // not muddied by the complexity of the actual API call.
      var response = await _fixture.DataGenerator.GetTestData();
      response.IsSuccessStatusCode.Should().BeTrue();

      var resultObj = await response.ConvertToType<SimplePingResult>();

      resultObj.Id.Should().Be(1);
      resultObj.Completed.Should().BeFalse();
    }
  }
}
