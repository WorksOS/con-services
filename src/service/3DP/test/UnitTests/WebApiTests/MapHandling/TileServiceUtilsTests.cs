using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
      Assert.IsTrue(Outside(bbox, points));
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
      Assert.IsFalse(Outside(bbox, points));
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
      Assert.IsFalse(Outside(bbox, points));
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
      Assert.IsFalse(Outside(bbox, points));
    }

    /// <summary>
    /// Determines if a polygon lies outside a bounding box.
    /// </summary>
    /// <param name="bbox">The bounding box</param>
    /// <param name="points">The polygon</param>
    /// <returns>True if the polygon is completely outside the bounding box otherwise false</returns>
    private bool Outside(MapBoundingBox bbox, List<WGSPoint> points)
    {
      return points.Min(p => p.Lat) > bbox.maxLat ||
             points.Max(p => p.Lat) < bbox.minLat ||
             points.Min(p => p.Lon) > bbox.maxLng ||
             points.Max(p => p.Lon) < bbox.minLng;
    }
  }
}
