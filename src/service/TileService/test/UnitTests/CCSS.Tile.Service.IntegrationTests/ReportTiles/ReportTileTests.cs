using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CCSS.IntegrationTests.Utils;
using CCSS.IntegrationTests.Utils.Extensions;
using CCSS.IntegrationTests.Utils.Types;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using XnaFan.ImageComparison.Netcore.Common;
using Xunit;

namespace CCSS.Tile.Service.IntegrationTests.ReportTiles
{
  public class ReportTileTests : IntegrationTestBase, IClassFixture<TestClientProviderFixture>
  {
    public ReportTileTests(TestClientProviderFixture testFixture)
    {
      restClient = testFixture.RestClient;
    }

    private static string GenerateOverlaysParam(string overlayType) 
    {
      var overlayParam = string.Empty;

      overlayParam = overlayType.Contains(",")
        ? overlayType.Split(',').Aggregate(overlayParam, (current, ot) => current + $"&overlays={ot.Trim()}")
        : $"&overlays={overlayType}";

      return overlayParam;
    }

    [Fact]
    public async Task Request_validation_should_fail_When_overlays_isnt_provided()
    {
      var uri = $"/api/v1/reporttiles/png?projectUid={ID.Project.DIMENSIONS}&width={256}&height={256}";

      var result = await SendAsync<ContractExecutionResult>(uri, HttpMethod.Get, customerUid: ID.Customer.DIMENSIONS, HttpStatusCode.BadRequest);

      Assert.Equal(-1, result.Code);
      Assert.Equal("At least one type of map tile overlay must be specified", result.Message);
    }

    [Fact]
    public async Task Request_validation_should_fail_When_mode_isnt_provided()
    {
      var uri = $"/api/v1/reporttiles/png?projectUid={ID.Project.DIMENSIONS}&width={256}&height={256}&overlays=ProductionData";

      var result = await SendAsync<ContractExecutionResult>(uri, HttpMethod.Get, customerUid: ID.Customer.DIMENSIONS, HttpStatusCode.BadRequest);

      Assert.Equal(-1, result.Code);
      Assert.Equal("Missing display mode parameter for production data overlay", result.Message);
    }

    [Fact]
    public async Task Request_validation_should_fail_When_missing_cutfill_design()
    {
      var uri = $"/api/v1/reporttiles/png?projectUid={ID.Project.DIMENSIONS}&width={256}&height={256}&overlays=ProductionData&mode=8";

      var result = await SendAsync<ContractExecutionResult>(uri, HttpMethod.Get, customerUid: ID.Customer.DIMENSIONS, HttpStatusCode.BadRequest);

      Assert.Equal(-1, result.Code);
      Assert.Equal("Missing design for cut-fill production data overlay", result.Message);
    }

    [Fact]
    public async Task Request_validation_should_fail_When_missing_volume_design()
    {
      var uri = $"/api/v1/reporttiles/png?projectUid={ID.Project.DIMENSIONS}&width={256}&height={256}&overlays=ProductionData&mode=8&volumeCalcType=DesignToGround";

      var result = await SendAsync<ContractExecutionResult>(uri, HttpMethod.Get, customerUid: ID.Customer.DIMENSIONS, HttpStatusCode.BadRequest);

      Assert.Equal(-1, result.Code);
      Assert.Equal("Missing design for summary volumes production data overlay", result.Message);
    }

    [Theory]
    [InlineData(256, 256, "BaseMap", "Missing map type parameter for base map overlay")]
    [InlineData(16, 16, "BaseMap", "Tile size must be between 64 and 2048 with a base map or 64 and 4096 otherwise")]
    public async Task Request_validation_should_fail_When_parameters_are_invalid(int width, int height, string overlays, string expectedMessage)
    {
      var uri = $"/api/v1/reporttiles/png?projectUid={ID.Project.DIMENSIONS}&width={width}&height={height}&overlays={overlays}";

      var result = await SendAsync<ContractExecutionResult>(uri, HttpMethod.Get, customerUid: ID.Customer.DIMENSIONS, HttpStatusCode.BadRequest);

      Assert.Equal(-1, result.Code);
      Assert.Equal(expectedMessage, result.Message);
    }

    [Theory]
    [InlineData("GroundToDesign", "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff", null, "Missing base filter for summary volumes production data overlay")]
    [InlineData("DesignToGround", null, "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff", "Missing top filter for summary volumes production data overlay")]
    public async Task Request_validation_should_fail_When_filters_are_invalid(string volumeCalcType, string topFilter, string baseFilter, string expectedMessage)
    {
      var uri = $"/api/v1/reporttiles/png?projectUid={ID.Project.DIMENSIONS}&width=256&height=256&overlays=ProductionData&mode=8&volumeCalcType={volumeCalcType}&volumeTopUid={topFilter}&volumeBaseUid={baseFilter}";

      var result = await SendAsync<ContractExecutionResult>(uri, HttpMethod.Get, customerUid: ID.Customer.DIMENSIONS, HttpStatusCode.BadRequest);

      Assert.Equal(-1, result.Code);
      Assert.Equal(expectedMessage, result.Message);
    }

    [Theory(Skip = "Reason")]
    //[InlineData("DxfLinework", "7b2bd262-8355-44ba-938a-d50f9712dafc", "DxfLinework", null, null, null, 1)]
    [InlineData("Alignments", null, "Alignments", null, null, null, 1)]
    [InlineData("ProjectBoundary", null, "ProjectBoundary", null, null, null, 1)]
    [InlineData("BaseMap", null, "BaseMap", null, null, null, 1)]
    [InlineData("BaseMapZH", null, "BaseMap", null, null, "zh-CN", 1)]
    [InlineData("BaseMapEN", null, "BaseMap", null, null, "en_US", 1)]
    [InlineData("Elevation", null, "ProductionData", null, 0, null, 5)]
    [InlineData("MDP", null, "ProductionData", null, 20, null, 3)]
    [InlineData("CMV", null, "ProductionData", null, 1, null, 3)]
    [InlineData("CMVchange", null, "ProductionData", null, 27, null, 3)]
    [InlineData("CMVsummary", null, "ProductionData", null, 13, null, 3)]
    [InlineData("Speed", null, "ProductionData", null, 26, null, 8)]
    [InlineData("TempSummary", null, "ProductionData", null, 10, null, 3)]
    [InlineData("TempDetail", null, "ProductionData", null, 30, null, 3)]
    [InlineData("PassCntDetailOverlay", null, "ProductionData,BaseMap,ProjectBoundary", "SATELLITE", 4, null, 10)]
    [InlineData("PassCntSummaryOverlay", null, "ProductionData,BaseMap,ProjectBoundary", "SATELLITE", 14, null, 10)]
    [InlineData("CMVchangeOverlay", null, "ProductionData,BaseMap,ProjectBoundary", "SATELLITE", 27, null, 10)]
    [InlineData("CMVsummaryOverlay", null, "ProductionData,BaseMap,ProjectBoundary", "SATELLITE", 13, null, 10)]
    [InlineData("SpeedOverlay", null, "ProductionData,BaseMap,ProjectBoundary", "SATELLITE", 26, null, 10)]
    [InlineData("TempSummaryOverlay", null, "ProductionData,BaseMap,ProjectBoundary", "SATELLITE", 10, null, 10)]
    [InlineData("TempDetailOverlay", null, "ProductionData,BaseMap,ProjectBoundary", "SATELLITE", 30, null, 10)]
    [InlineData("ElevationOverlay", null, "ProductionData,BaseMap,ProjectBoundary", "SATELLITE", 0, null, 10)]
    [InlineData("MDPOverlay", null, "ProductionData,BaseMap,ProjectBoundary", "SATELLITE", 20, null, 10)]
    [InlineData("CMVOverlay", null, "ProductionData,BaseMap,ProjectBoundary", "SATELLITE", 1, null, 10)]
    [InlineData("ElevationOverlayAll", null, "AllOverlays", "HYBRID", 0, null, 3)]
    [InlineData("CMVchangeOverlayAll", null, "AllOverlays", "HYBRID", 27, null, 3)]
    [InlineData("CMVsummaryOverlayAll", null, "AllOverlays", "HYBRID", 13, null, 3)]
    [InlineData("SpeedOverlayAll", null, "AllOverlays", "HYBRID", 26, null, 8)]
    [InlineData("TempSummaryOverlayAll", null, "AllOverlays", "HYBRID", 10, null, 3)]
    [InlineData("TempDetailOverlayAll", null, "AllOverlays", "HYBRID", 30, null, 3)]
    [InlineData("MDPOverlayAll", null, "AllOverlays", "HYBRID", 20, null, 3)]
    [InlineData("CMVOverlayAll", null, "AllOverlays", "HYBRID", 1, null, 3)]
    [InlineData("PCWithAlignOverlayAll", "2811c7c3-d270-4d63-97e2-fc3340bf6c6b", "AllOverlays", "HYBRID", 4, null, 10)]
    [InlineData("ElevWithAlignOverlayAll", "2811c7c3-d270-4d63-97e2-fc3340bf6c6b", "AllOverlays", "HYBRID", 0, null, 3)]
    [InlineData("TempWithAlignOverlayAll", "2811c7c3-d270-4d63-97e2-fc3340bf6c6b", "AllOverlays", "HYBRID", 10, null, 3)]
    public async Task Report_tiles_should_match(string resultName, string filterUid, string overlayType, string mapType, int? mode, string language, double tollerance)
    {
      var modeParam = mode == null ? null : $"&mode={mode}";
      var languageParam = language == null ? null : $"&language={language}";
      var mapTypeParam = mapType == null ? null : $"&mapType={mapType}";
      var filterParam = filterUid == null ? null : $"&filterUid={filterUid}";
      var overlaysParam = GenerateOverlaysParam(overlayType);

      var uri = $"/api/v1/reporttiles/png?projectUid={ID.Project.DIMENSIONS}&width=256&height=256{overlaysParam}{modeParam}{filterParam}{mapTypeParam}{languageParam}";

      var response = await restClient.SendAsync(uri, HttpMethod.Get, customerUid: ID.Customer.DIMENSIONS, acceptHeader: MediaTypes.PNG);

      response.EnsureSuccessStatusCode();

      var result = await response.ToByteArray();
      var json = await ReadJsonFile("LineworkTiles", resultName);

      Assert.True(json.TryGetValue("Linework", out var jToken));
      Assert.NotNull(jToken);

      var expectedData = jToken.ToObject<byte[]>();

      Assert.True(CommonUtils.CompareImages(resultName, tollerance, expectedData, result, out var actualDiff),
                  $"Linework tile for '{resultName}' doesn't match. Allowed tollerance {tollerance}, actual difference is {actualDiff}");
    }

    [Theory(Skip="Reason")]
    [InlineData("CutFill", "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff", null, null, null, "ProductionData", null, 8, 5, null)]
    [InlineData("CutFillOverlay", "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff", null, null, null, "ProductionData,BaseMap,ProjectBoundary", "MAP", 8, 3, null)]
    [InlineData("CutFillTerrain", "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff", null, null, null, "ProductionData,BaseMap,ProjectBoundary", "TERRAIN", 8, 3, null)]
    [InlineData("CutFillOverlayAll", "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff", null, null, null, "AllOverlays", "HYBRID", 8, 3, null)]
    [InlineData("D2GOverlayAll", null, "DesignToGround", "a54e5945-1aaa-4921-9cc1-c9d8c0a343d3", "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff", "AllOverlays", "HYBRID", 8, 3, null)]
    [InlineData("DesignToGround", null, "DesignToGround", "a54e5945-1aaa-4921-9cc1-c9d8c0a343d3", "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff", "ProductionData", null, 8, 3, null)]
    [InlineData("GroundToGround", null, "DesignToGround", "A40814AA-9CDB-4981-9A21-96EA30FFECDD", "F07ED071-F8A1-42C3-804A-1BDE7A78BE5B", "ProductionData", null, 8, 3, null)]
    [InlineData("GroundToDesign", null, "DesignToGround", "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff", "9c27697f-ea6d-478a-a168-ed20d6cd9a22", "ProductionData", null, 8, 3, null)]
    [InlineData("G2DOverlayAll", null, "DesignToGround", "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff", "9c27697f-ea6d-478a-a168-ed20d6cd9a22", "AllOverlays", "HYBRID", 8, 3, null)]
    [InlineData("CFillWithAlignOverlayAll", "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff", null, null, null, "AllOverlays", "HYBRID", 8, 3, "2811c7c3-d270-4d63-97e2-fc3340bf6c6b")]
    public async Task Report_cutfill_and_volume_tiles(string resultName, string cutFillDesignUid, string volumeCalcType, string volumeTopUid, string volumeBaseUid, string overlayType, string mapType, int mode, double tollerance, string filterUid)
    {
      var filterUidParam = filterUid == null ? null : $"&mode={filterUid}";
      var mapTypeParam = mapType == null ? null : $"&mapType={mapType}";
      var cutFillDesignUidParam = cutFillDesignUid == null ? null : $"&cutFillDesignUid={cutFillDesignUid}";
      var volumeCalcTypeParam = volumeCalcType == null ? null : $"&volumeCalcType={volumeCalcType}";
      var volumeTopUidParam = volumeTopUid == null ? null : $"&volumeTopUid={volumeTopUid}";
      var volumeBaseUidParam = volumeBaseUid == null ? null : $"&volumeBaseUid={volumeBaseUid}";
      var overlaysParam = GenerateOverlaysParam(overlayType);

      var uri = $"/api/v1/reporttiles/png?projectUid={ID.Project.DIMENSIONS}&width=256&height=256{overlaysParam}&mode={mode}{volumeCalcTypeParam}{cutFillDesignUidParam}{mapTypeParam}{filterUidParam}{volumeTopUidParam}{volumeBaseUidParam}";

      var response = await restClient.SendAsync(uri, HttpMethod.Get, customerUid: ID.Customer.DIMENSIONS, acceptHeader: MediaTypes.PNG);

      response.EnsureSuccessStatusCode();

      var result = await response.ToByteArray();
      var json = await ReadJsonFile("LineworkTiles", resultName);

      Assert.True(json.TryGetValue("Linework", out var jToken));
      Assert.NotNull(jToken);

      var expectedData = jToken.ToObject<byte[]>();

      Assert.True(CommonUtils.CompareImages(resultName, tollerance, expectedData, result, out var actualDiff),
                  $"Linework tile for '{resultName}' doesn't match. Allowed tollerance {tollerance}, actual difference is {actualDiff}");
    }

    [Fact(Skip="Reason")]
    public async Task Large_report_tiles()
    {
      const string resultName = "CMVLarge";
      const double tollerance = 10;
      var overlaysParam = GenerateOverlaysParam("ProductionData,BaseMap,ProjectBoundary");

      var uri = $"/api/v1/reporttiles/png?projectUid={ID.Project.DIMENSIONS}&width=256&height=256{overlaysParam}&mode=1&mapType=SATELLITE";

      var response = await restClient.SendAsync(uri, HttpMethod.Get, customerUid: ID.Customer.DIMENSIONS, acceptHeader: MediaTypes.PNG);

      response.EnsureSuccessStatusCode();

      var result = await response.ToByteArray();
      var json = await ReadJsonFile("LineworkTiles", resultName);

      Assert.True(json.TryGetValue("Linework", out var jToken));
      Assert.NotNull(jToken);

      var expectedData = jToken.ToObject<byte[]>();

      Assert.True(CommonUtils.CompareImages(resultName, tollerance, expectedData, result, out var actualDiff),
                  $"Linework tile for '{resultName}' doesn't match. Allowed tollerance {tollerance}, actual difference is {actualDiff}");
    }

    [Theory(Skip="Reason")]
    [InlineData("GroundToGroundExplicit", true, 1)]
    [InlineData("GroundToGround", false, 1)]
    public async Task Report_cutfill_and_volume_tiles_with_explicitFilters(string resultName, bool explicitFilters, double tollerance)
    {
      var uri = $"/api/v1/reporttiles/png?projectUid={ID.Project.DIMENSIONS}&width=256&height=256&overlays=ProductionData&mode=8&volumeTopUid=A40814AA-9CDB-4981-9A21-96EA30FFECDD&volumeBaseUid=F07ED071-F8A1-42C3-804A-1BDE7A78BE5B&volumeCalcType=GroundToGround&explicitFilters={explicitFilters}";

      var response = await restClient.SendAsync(uri, HttpMethod.Get, customerUid: ID.Customer.DIMENSIONS, acceptHeader: MediaTypes.PNG);

      response.EnsureSuccessStatusCode();

      var result = await response.ToByteArray();
      var json = await ReadJsonFile("LineworkTiles", resultName);

      Assert.True(json.TryGetValue("Linework", out var jToken));
      Assert.NotNull(jToken);

      var expectedData = jToken.ToObject<byte[]>();

      Assert.True(CommonUtils.CompareImages(resultName, tollerance, expectedData, result, out var actualDiff),
                  $"Linework tile for '{resultName}' doesn't match. Allowed tollerance {tollerance}, actual difference is {actualDiff}");
    }
  }
}
