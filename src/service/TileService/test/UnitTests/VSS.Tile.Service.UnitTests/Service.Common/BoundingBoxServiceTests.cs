using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.MasterData.Models.Models;
using VSS.Serilog.Extensions;
using VSS.Tile.Service.Common.Models;
using VSS.Tile.Service.Common.Services;
using Xunit;

namespace VSS.Tile.Service.UnitTests.Service.Common
{
  public class BoundingBoxServiceTests
  {
    public ILoggerFactory loggerFactory;

    public BoundingBoxServiceTests()
    {
      var serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Tile.Service.UnitTests.log")))
        .BuildServiceProvider();

      loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    }

    [Theory]
    [InlineData(12561,24307,16, -110.994873046875, 42.08599350447723, -111.0003662109375, 42.081916678306314)]
    public void ConvertXYZToBBox(int x, int y, int z, double lon_nw, double lat_nw, double lon_se, double lat_se)
    {
      var bbox = WebMercatorProjection.FromXyzToBoundingBox2DLatLon(x, y, z);

      bbox.TopRightLon.LonRadiansToDegrees().Should().BeApproximately(lon_nw, 0.0001);
      bbox.TopRightLat.LatRadiansToDegrees().Should().BeApproximately(lat_nw, 0.0001);
      bbox.BottomLeftLon.LonRadiansToDegrees().Should().BeApproximately(lon_se, 0.0001);
      bbox.BottomLeftLat.LatRadiansToDegrees().Should().BeApproximately(lat_se, 0.0001);
    }


    /// <summary>
    /// Bounding boxes with zero area (i.e a point) 
    /// </summary>
    [Theory]
    [InlineData(4096, 4096, false, 4096, 4096, 1.5269, 1.1341, -1.4009, 1.1341)]//tile larger than bbox
    public void TestZeroAreaBoundingBoxes(int tileWidth, int tileHeight, bool addMargin, int expectedWidth, int expectedHeight,
      double expectedMinLat, double expectedMinLng, double expectedMaxLat, double expectedMaxLng)
    {
      var minLat = 0.63137;//36.175°
      var minLng = -2.00748;//-115.020°
      var maxLat = 0.63137;//36.178°
      var maxLng = -2.00748;//-115.018°
      MapBoundingBox bbox = new MapBoundingBox
      {
        minLat = minLat,
        minLng = minLng,
        maxLat = maxLat,
        maxLng = maxLng
      };

      var service = new BoundingBoxService(loggerFactory);
      //numTiles = 1048576 for Z10
      MapParameters parameters = new MapParameters
      {
        bbox = bbox,
        numTiles = 1048576,
        zoomLevel = 10,
        mapWidth = tileWidth,
        mapHeight = tileHeight,
        addMargin = addMargin
      };
      service.AdjustBoundingBoxToFit(parameters);
      parameters.mapWidth.Should().Be(expectedWidth);
      parameters.mapHeight.Should().Be(expectedHeight);
      parameters.bbox.minLat.Should().BeApproximately(expectedMinLat, 0.0001);
      parameters.bbox.minLng.Should().BeApproximately(expectedMinLng, 0.0001);
      parameters.bbox.maxLat.Should().BeApproximately(expectedMaxLat, 0.0001);
      parameters.bbox.maxLng.Should().BeApproximately(expectedMaxLng, 0.0001);
    }


    [Theory]
    [InlineData(4096, 4096, false, 4096, 4096, 0.63136, -2.00751, 0.63144, -2.00741)]//tile larger than bbox
    [InlineData(2048, 2048, false, 2646, 2646, 0.63137, -2.00749, 0.63142, -2.00743)]//square tile smaller than bbox 
    [InlineData(2048, 1024, false, 5292, 2646, 0.63137, -2.00752, 0.63142, -2.00740)]//rectangular tile smaller than bbox
    [InlineData(4096, 4096, true, 4096, 4096, 0.63136, -2.00751, 0.63144, -2.00741)]//tile larger than bbox
    [InlineData(2048, 2048, true, 2910, 2910, 0.63137, -2.00749, 0.63142, -2.00743)]//square tile smaller than bbox 
    [InlineData(2048, 1024, true, 5820, 2910, 0.63137, -2.00752, 0.63142, -2.00740)]//rectangular tile smaller than bbox
    public void ShouldAdjustBoundingBoxToFit(int tileWidth, int tileHeight, bool addMargin, int expectedWidth, int expectedHeight,
  double expectedMinLat, double expectedMinLng, double expectedMaxLat, double expectedMaxLng)
    {
      var minLat = 0.63137;//36.175°
      var minLng = -2.00748;//-115.020°
      var maxLat = 0.63142;//36.178°
      var maxLng = -2.00744;//-115.018°
      MapBoundingBox bbox = new MapBoundingBox
      {
        minLat = minLat,
        minLng = minLng,
        maxLat = maxLat,
        maxLng = maxLng
      };

      var service = new BoundingBoxService(loggerFactory);
      //numTiles = 1048576 for Z10
      MapParameters parameters = new MapParameters
      {
        bbox = bbox,
        numTiles = 1048576,
        zoomLevel = 10,
        mapWidth = tileWidth,
        mapHeight = tileHeight,
        addMargin = addMargin
      };
      service.AdjustBoundingBoxToFit(parameters);

      parameters.mapWidth.Should().Be(expectedWidth);
      parameters.mapHeight.Should().Be(expectedHeight);
      parameters.bbox.minLat.Should().BeApproximately(expectedMinLat, 0.0001);
      parameters.bbox.minLng.Should().BeApproximately(expectedMinLng, 0.0001);
      parameters.bbox.maxLat.Should().BeApproximately(expectedMaxLat, 0.0001);
      parameters.bbox.maxLng.Should().BeApproximately(expectedMaxLng, 0.0001);

    }
  }
}
