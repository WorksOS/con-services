using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CCSS.IntegrationTests.Utils;
using CCSS.IntegrationTests.Utils.Extensions;
using CCSS.IntegrationTests.Utils.Types;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using XnaFan.ImageComparison.Netcore.Common;
using Xunit;

namespace CCSS.Tile.Service.IntegrationTests.LineworkTiles
{
  public class LineworkTileTests : IntegrationTestBase, IClassFixture<TestClientProviderFixture>
  {
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
      var uri = $"/api/v1/lineworktiles3d/20/12345/54321.png?projectUid={ID.Project.DIMENSIONS}&width=256&height=256&fileType={fileType}";

      var result = await SendAsync<ContractExecutionResult>(uri, HttpMethod.Get, customerUid: ID.Customer.DIMENSIONS, HttpStatusCode.BadRequest);

      Assert.Equal(-1, result.Code);
      Assert.Equal(expectedMessage, result.Message);
    }

    [Theory(Skip = "Ignore until CCSSSCON-246 is completed.")]
    [InlineData("Linework", 15, 12844, 5914, 1.00, "Linework.json")]
    [InlineData("Alignment", 18, 102753, 47317, 1.00, "Linework.json")]
    [InlineData("GeoTiff", 16, 41583, 64159, 1.00, "Linework.json")]
    public async Task Linework_Tile_Should_Match(string fileType, int zoomLevel, int tileY, int tileX, double tollerance, string expectedFileData)
    {
      var uri = $"/api/v1/lineworktiles3d/{zoomLevel}/{tileY}/{tileX}.png?projectUid={ID.Project.DIMENSIONS}&width=256&height=256&fileType={fileType}";

      var response = await restClient.SendAsync(uri, HttpMethod.Get, customerUid: ID.Customer.DIMENSIONS, acceptHeader: MediaTypes.PNG);

      response.EnsureSuccessStatusCode();

      var result = await response.ToByteArray();
      var json = await ReadJsonFile("LineworkTiles", expectedFileData);

      Assert.True(json.TryGetValue("Linework", out var jToken));
      Assert.NotNull(jToken);

      var expectedData = jToken.ToObject<byte[]>();

      Assert.True(CommonUtils.CompareImages(fileType, tollerance, expectedData, result, out var actualDiff),
                  $"Linework tile for '{fileType}' doesn't match. Allowed tollerance {tollerance}, actual difference is {actualDiff}");
    }
  }
}
