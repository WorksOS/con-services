using System.Net.Http;
using System.Threading.Tasks;
using CCSS.IntegrationTests.Utils;
using CCSS.IntegrationTests.Utils.Extensions;
using CCSS.IntegrationTests.Utils.Types;
using XnaFan.ImageComparison.Netcore.Common;
using Xunit;

namespace CCSS.Tile.Service.IntegrationTests.Thumbnails
{
  public class ProjectThumbnailsTests : IntegrationTestBase, IClassFixture<TestClientProviderFixture>
  {
    public ProjectThumbnailsTests(TestClientProviderFixture testFixture)
    {
      restClient = testFixture.RestClient;
    }

    [Theory(Skip="Ignore until CCSSSCON-246 is completed.")]
    [InlineData("/api/v1/projectthumbnail3d/png", MediaTypes.PNG, ID.Project.DIMENSIONS, "ProductionData")]
    [InlineData("/api/v1/projectthumbnail3d/base64", null, ID.Project.DIMENSIONS, "ProductionData")]
    [InlineData("/api/v1/projectthumbnail3d/png", MediaTypes.PNG, ID.Project.DIMENSIONS_EMPTY_PROJECT_UID, "NoProductionData")]
    [InlineData("/api/v1/projectthumbnail/png", MediaTypes.PNG, ID.Project.DIMENSIONS, "ProjectBoundaryOnly")]
    [InlineData("/api/v1/projectthumbnail/base64", null, ID.Project.DIMENSIONS, "ProjectBoundaryOnly")]
    [InlineData("/api/v1/projectthumbnail2d/png", MediaTypes.PNG, ID.Project.DIMENSIONS, "LoadDumpData")]
    [InlineData("/api/v1/projectthumbnail2d/base64", null, ID.Project.DIMENSIONS, "LoadDumpData")]
    
    public async Task Request_should_return_the_expected_thumbnail_response(string url, string acceptHeader, string projectUid, string expectedFileData)
    {
      var uri = $"{url}?projectUid={projectUid}";

      var response = await restClient.SendAsync(uri, HttpMethod.Get, customerUid: ID.Customer.DIMENSIONS, acceptHeader: acceptHeader);

      response.EnsureSuccessStatusCode();

      var result = await response.ToByteArray();
      var json = await ReadJsonFile("LineworkTiles", expectedFileData);

      Assert.True(json.TryGetValue("Linework", out var jToken));
      Assert.NotNull(jToken);

      var expectedData = jToken.ToObject<byte[]>();

      double tollerance = 3;

      Assert.True(CommonUtils.CompareImages(expectedFileData, tollerance, expectedData, result, out var actualDiff),
                  $"Thumbnail for '{expectedFileData}' doesn't match. Allowed tollerance {tollerance}, actual difference is {actualDiff}");
    }
  }
}
