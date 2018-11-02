using FluentAssertions;
using Microsoft.Extensions.Logging;
using VSS.Tile.Service.Common.Models;
using VSS.Tile.Service.Common.Services;
using Xunit;

namespace VSS.Tile.Service.Common.Tests
{
  public class BoundingBoxServiceTests
  {
    public ILoggerFactory loggerFactory;

    public BoundingBoxServiceTests()
    {
      loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
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
