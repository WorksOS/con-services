using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CCSS.IntegrationTests.Utils;
using CCSS.IntegrationTests.Utils.Extensions;
using CCSS.IntegrationTests.Utils.Types;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using Xunit;

namespace CCSS.Tile.Service.IntegrationTests.LineworkTile
{
  public class LineworkTileTests : IClassFixture<TestClientProviderFixture>
  {
    private const string HostAddress = "http://localhost";
    private readonly IRestClient restClient;

    public LineworkTileTests(TestClientProviderFixture testFixture)
    {
      restClient = testFixture.RestClient;
    }

    [Theory]
    [InlineData(null, "Missing file type")]
    [InlineData("invalid", "Invalid file type invalid")]
    [InlineData("ReferenceSurface", "Unsupported file type ReferenceSurface")]
    public async Task FileType_Validation_Should_Fail(string fileType, string expectedMessage)
    {
      var uri = $@"{HostAddress}/api/v1/lineworktiles3d/20/12345/54321.png?projectUid={ID.Project.Unknown1}&width=256&height=256&fileType={fileType}";
      var response = await restClient.SendHttpClientRequest(uri, HttpMethod.Get, customerUid: ID.Customer.Unknown1);

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

      var result = await response.ConvertToType<ContractExecutionResult>();

      Assert.Equal(-1, result.Code);
      Assert.Equal(expectedMessage, result.Message);
    }
  }
}
