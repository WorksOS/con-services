using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.MasterData.Models.Models;
using Point = VSS.MasterData.Models.Models.Point;

namespace VSS.Productivity3D.WebApiTests.MapHandling
{
  [TestClass]
  public class TileServiceUtilsTests
  {

    [TestMethod]
    [DataRow(36.210, -115.025, 1514027.25F, 3288030F)]
    [DataRow(36.205, -115.029, 1513934F, 3288174.5F)]
    [DataRow(36.200, -115.018, 1514190.38F, 3288318.75F)]
    public void CanConvertLatLngToPixel(double latDegrees, double lngDegrees, float xExpected, float yExpected)
    {
      var pixelPoint = TileServiceUtils.LatLngToPixel(latDegrees.LatDegreesToRadians(), lngDegrees.LonDegreesToRadians(), 32768);
      Assert.AreEqual(xExpected, pixelPoint.x, 0.1);
      Assert.AreEqual(yExpected, pixelPoint.y, 0.1);
    }

    [TestMethod]
    public void RoundingZoomLevelTest()
    {
      double diff1 = 0.000365585907798893;
      double diff2 = 0.000766990393942846;

      double diff1_1 = 0.000365339467977011;
      double diff2_1 = 0.000766990393942818;

      var res1 = TileServiceUtils.CalculateZoomLevel(diff1, diff2);
      var res2 = TileServiceUtils.CalculateZoomLevel(diff1_1, diff2_1);

      Assert.AreEqual(res1,res2);
      Assert.AreEqual(13,res1);
    }

    [TestMethod]
    public void CanConvertLatLngToPixelOffset()
    {
      var latLngs = new List<WGSPoint>
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
        Assert.AreEqual(expectedPoints[i], pixelPoints[i]);
      }
    }

    [TestMethod]
    public void OverlayTilesReturnsTileForEmptyList()
    {
      var mapParameters = new MapParameters {mapWidth = 4, mapHeight = 4};
      var result = TileServiceUtils.OverlayTiles(mapParameters, new Dictionary<TileOverlayType, byte[]>());
      var expectedResult = new byte[]
      {
        137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82, 0, 0, 0, 4, 0, 0, 0, 4, 8, 6, 0, 0, 0, 169, 241,
        158, 126, 0, 0, 0, 1, 115, 82, 71, 66, 0, 174, 206, 28, 233, 0, 0, 0, 4, 103, 65, 77, 65, 0, 0, 177, 143, 11,
        252, 97, 5, 0, 0, 0, 9, 112, 72, 89, 115, 0, 0, 14, 195, 0, 0, 14, 195, 1, 199, 111, 168, 100, 0, 0, 0, 13,
        73, 68, 65, 84, 24, 87, 99, 160, 24, 48, 48, 0, 0, 0, 68, 0, 1, 190, 92, 113, 234, 0, 0, 0, 0, 73, 69, 78, 68,
        174, 66, 96, 130
      };
      for (int i = 0; i < expectedResult.Length; i++)
      {
        Assert.AreEqual(expectedResult[i], result[i]);
      }
    }

    [TestMethod]
    public void CanCalculateZoomLevel()
    {
      Assert.AreEqual(1, TileServiceUtils.CalculateZoomLevel(Math.PI / 2, Math.PI));
      Assert.AreEqual(11, TileServiceUtils.CalculateZoomLevel(Math.PI / 2000, Math.PI / 1000));
      Assert.AreEqual(21, TileServiceUtils.CalculateZoomLevel(Math.PI / 2000000, Math.PI / 1000000));
      Assert.AreEqual(24, TileServiceUtils.CalculateZoomLevel(Math.PI / 2000000000, Math.PI / 1000000000));
    }


    [TestMethod]
    public void CanCalculateNumberOfTiles()
    {
      Assert.AreEqual(1024, TileServiceUtils.NumberOfTiles(10));
    }

    [TestMethod]
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
      Assert.IsTrue(TileServiceUtils.Outside(bbox, points));
    }

    [TestMethod]
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
      Assert.IsFalse(TileServiceUtils.Outside(bbox, points));
    }

    [TestMethod]
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
      Assert.IsFalse(TileServiceUtils.Outside(bbox, points));
    }

    [TestMethod]
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
      Assert.IsFalse(TileServiceUtils.Outside(bbox, points));
    }
  }
}
