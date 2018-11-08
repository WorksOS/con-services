using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using Point = VSS.MasterData.Models.Models.Point;

namespace VSS.Productivity3D.WebApiTests.Notification.Helpers
{
  [TestClass]
  public class WebMercatorProjectionTests
  {
    [TestMethod]
    [DataRow((double)0, 0)]
    [DataRow((double)100, 1.74532925199433)]
    [DataRow((double)42, 0.733038285837618)]
    [DataRow(179.5, 3.13286600732982)]
    [DataRow((double)180, Math.PI)]
    public void Point_DegreesToRadians(double deg, double expectedResult)
    {
      Assert.AreEqual(expectedResult, deg.LonDegreesToRadians(), 0.00000001);
    }

    [TestMethod]
    [DataRow((double)0, 0)]
    [DataRow((double)42, 2406.42273954946)]
    [DataRow((double)100, 5729.57795130823)]
    [DataRow(456.32, 26145.2101074097)]
    public void Point_RadianToDegreesPoint(double rad, double expectedResult)
    {
      Assert.AreEqual(expectedResult, rad.LatRadiansToDegrees(), 0.00000001);
    }

    [TestMethod]
    [DataRow(0, 0, 128, 128)]
    [DataRow(4, 6, 132.266666666667, 125.153242156653)]
    public void Point_FromLatLngToPoints(int x, int y, double expectedX, double expectedY)
    {
      var result = WebMercatorProjection.FromLatLngToPoint(new Point(x, y));

      Assert.AreEqual(expectedX, result.Longitude, 0.00000001);
      Assert.AreEqual(expectedY, result.Latitude, 0.00000001);
    }

    [TestMethod]
    [DataRow((double)128, (double)128, 0, 0)]
    [DataRow(132.26666667, 125.15324216, -4.0032532125, -5.98906374664545)]
    public void Point_FromPointToLatLng(double x, double y, double expectedX, double expectedY)
    {
      var result = WebMercatorProjection.FromPointToLatLng(new Point(x, y));

      Assert.AreEqual(expectedX, result.Longitude, 0.00000001);
      Assert.AreEqual(expectedY, result.Latitude, 0.00000001);
    }

    [TestMethod]
    [DataRow(0, 0, 0, 0, 0)]
    [DataRow(4, 6, 1, 132.266666666667, 125.153242156653)]
    [DataRow(4, 6, 2, 264.533333333333, 250.306484313305)]
    public void Point_LatLngToPixel(int x, int y, long numTiles, double expectedX, double expectedY)
    {
      var result = WebMercatorProjection.LatLngToPixel(new Point(x, y), numTiles);

      Assert.AreEqual(expectedX, result.Longitude, 0.00000001);
      Assert.AreEqual(expectedY, result.Latitude, 0.00000001);
    }
  }
}
