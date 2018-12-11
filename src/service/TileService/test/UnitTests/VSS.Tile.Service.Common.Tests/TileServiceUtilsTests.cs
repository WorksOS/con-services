using System;
using System.Collections.Generic;
using Point = VSS.MasterData.Models.Models.Point;
using VSS.MasterData.Models.Models;
using Xunit;
using VSS.Tile.Service.Common.Helpers;
using VSS.Tile.Service.Common.Models;
using VSS.Tile.Service.Common.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace VSS.Tile.Service.Common.Tests
{
  public class TileServiceUtilsTests
  {
    [Theory]
    [InlineData(36.210, -115.025, 1514027.25F, 3288030F)]
    [InlineData(36.205, -115.029, 1513934F, 3288174.5F)]
    [InlineData(36.200, -115.018, 1514190.38F, 3288318.75F)]
    public void CanConvertLatLngToPixel(double latDegrees, double lngDegrees, float xExpected, float yExpected)
    {
      var pixelPoint = TileServiceUtils.LatLngToPixel(latDegrees.LatDegreesToRadians(), lngDegrees.LonDegreesToRadians(), 32768);
      Assert.Equal(xExpected, pixelPoint.x, 0);
      Assert.Equal(yExpected, pixelPoint.y, 0);
    }

    [Fact]
    public void RoundingZoomLevelTest()
    {
      double diff1 = 0.000365585907798893;
      double diff2 = 0.000766990393942846;

      double diff1_1 = 0.000365339467977011;
      double diff2_1 = 0.000766990393942818;

      var res1 = TileServiceUtils.CalculateZoomLevel(diff1, diff2);
      var res2 = TileServiceUtils.CalculateZoomLevel(diff1_1, diff2_1);

      Assert.Equal(res1,res2);
      Assert.Equal(13,res1);

    }

    [Fact]
    public void CanConvertLatLngToPixelOffset()
    {
      List<WGSPoint> latLngs = new List<WGSPoint>
      {
        new WGSPoint(36.210.LatDegreesToRadians(), -115.025.LonDegreesToRadians()),
        new WGSPoint(36.205.LatDegreesToRadians(), -115.029.LonDegreesToRadians()),
        new WGSPoint(36.200.LatDegreesToRadians(), -115.018.LonDegreesToRadians())
      };
      var topLeft = new Point(100, 250);
      var pixelPoints = TileServiceUtils.LatLngToPixelOffset(latLngs, topLeft, 32768);

      var expectedPoints = new PointF[3]
      {
        new PointF{X = 1513777.25F, Y = 3287930F},
        new PointF{X = 1513684F, Y = 3288074.5F},
        new PointF{X = 1513940.38F, Y = 3288218.75F},
      };

      for (int i = 0; i < 3; i++)
      {
        Assert.Equal(expectedPoints[i], pixelPoints[i]);
      }
    }

    [Fact]
    public void OverlayTilesReturnsTileForEmptyList()
    {
      var mapParameters = new MapParameters {mapWidth = 4, mapHeight = 4};
      var result = TileServiceUtils.OverlayTiles(mapParameters, new Dictionary<TileOverlayType, byte[]>());

      byte[] expectedResult = null;
      using (Image<Rgba32> bitmap = new Image<Rgba32>(mapParameters.mapWidth, mapParameters.mapHeight))
      {
        expectedResult = bitmap.BitmapToByteArray();
      }
  
      for (int i = 0; i < expectedResult.Length; i++)
      {
        Assert.Equal(expectedResult[i], result[i]);
      }
    }

    [Fact]
    public void CanCalculateZoomLevel()
    {
      Assert.Equal(1, TileServiceUtils.CalculateZoomLevel(Math.PI / 2, Math.PI));
      Assert.Equal(11, TileServiceUtils.CalculateZoomLevel(Math.PI / 2000, Math.PI / 1000));
      Assert.Equal(21, TileServiceUtils.CalculateZoomLevel(Math.PI / 2000000, Math.PI / 1000000));
      Assert.Equal(24, TileServiceUtils.CalculateZoomLevel(Math.PI / 2000000000, Math.PI / 1000000000));
    }


    [Fact]
    public void CanCalculateNumberOfTiles()
    {
      Assert.Equal(1024, TileServiceUtils.NumberOfTiles(10));
    }

    [Fact]
    public void PolygonOutsideBoundingBox()
    {
      var bbox = new MapBoundingBox
      {
        minLat = 36.0, minLng = -115.9, maxLat = 36.5, maxLng = -115.0
      };
      var points = new List<WGSPoint>
      {
        new WGSPoint(35.0, -116.0),
        new WGSPoint(35.5, -116.0),
        new WGSPoint(35.5, -116.5),
        new WGSPoint(35.0, -116.5),
        new WGSPoint(35.0, -116.0),
      };
      Assert.True(TileServiceUtils.Outside(bbox, points));
    }

    [Fact]
    public void PolygonInsideBoundingBox()
    {
      var bbox = new MapBoundingBox
      {
        minLat = 36.0,
        minLng = -115.9,
        maxLat = 36.5,
        maxLng = -115.0
      };
      var points = new List<WGSPoint>
      {
        new WGSPoint(36.1, -115.5),
        new WGSPoint(36.3, -115.5),
        new WGSPoint(36.3, -115.7),
        new WGSPoint(36.1, -115.7),
        new WGSPoint(36.1, -115.5),
      };
      Assert.False(TileServiceUtils.Outside(bbox, points));
    }

    [Fact]
    public void PolygonIntersectsBoundingBox()
    {
      var bbox = new MapBoundingBox
      {
        minLat = 36.0,
        minLng = -115.9,
        maxLat = 36.5,
        maxLng = -115.0
      };
      var points = new List<WGSPoint>
      {
        new WGSPoint(35.9, -115.5),
        new WGSPoint(36.3, -115.5),
        new WGSPoint(36.3, -115.7),
        new WGSPoint(35.9, -115.7),
        new WGSPoint(35.9, -115.5),
      };
      Assert.False(TileServiceUtils.Outside(bbox, points));
    }

    [Fact]
    public void PolygonEnvelopsBoundingBox()
    {
      var bbox = new MapBoundingBox
      {
        minLat = 36.0,
        minLng = -115.9,
        maxLat = 36.5,
        maxLng = -115.0
      };
      var points = new List<WGSPoint>
      {
        new WGSPoint(35.9, -116.0),
        new WGSPoint(36.6, -116.0),
        new WGSPoint(36.6, -114.9),
        new WGSPoint(35.9, -114.9),
        new WGSPoint(35.9, -116.0),
      };
      Assert.False(TileServiceUtils.Outside(bbox, points));
    }


  }
}
