using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.WebApi.Models.Notification.Helpers;

namespace VSS.Productivity3D.WebApiTests.Notification.Helpers
{
  [TestClass]
  public class WebMercatorProjectionTests
  {
    [TestMethod]
    [DataRow((double)0, 0)]
    [DataRow((double)100, 1.74532925199433)]
    [DataRow((double)42, 0.733038285837618)]
    public void Point_DegreesToRadians(double deg, double expectedResult)
    {
      Assert.AreEqual(expectedResult, WebMercatorProjection.DegreesToRadians(deg), 0.00000001);
    }

    [TestMethod]
    [DataRow((double)0, 0)]
    [DataRow((double)42, 2406.42273954946)]
    [DataRow((double)100, 5729.57795130823)]
    public void Point_RadianToDegreesPoint(double rad, double expectedResult)
    {
      Assert.AreEqual(expectedResult, WebMercatorProjection.RadiansToDegrees(rad), 0.00000001);
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
    [DataRow(0, 0, 0, 0, 0)]
    [DataRow(4, 6, 1, 132.266666666667, 125.153242156653)]
    [DataRow(4, 6, 2, 264.533333333333, 250.306484313305)]
    public void Point_LatLngToPixel(int x, int y, int numTiles, double expectedX, double expectedY)
    {
      var result = WebMercatorProjection.LatLngToPixel(new Point(x, y), numTiles);

      Assert.AreEqual(expectedX, result.Longitude, 0.00000001);
      Assert.AreEqual(expectedY, result.Latitude, 0.00000001);
    }
  }
}