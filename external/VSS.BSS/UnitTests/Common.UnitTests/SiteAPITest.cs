using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;
using ED=VSS.Nighthawk.ExternalDataTypes.Enumerations;
namespace UnitTests
{
  [TestClass()]
  public class SiteAPITest : UnitTestBase
  {
    [DatabaseTest]
    [TestMethod]
    [Ignore]
    [ExpectedException(typeof(InvalidOperationException))]
    public void SiteAPI_CheckValidSite_FailureDuplicateSiteName()
    {

    }

    [DatabaseTest]
    [TestMethod]
    [Ignore]
    public void SiteAPI_SetSiteVisibility_Success()
    {

    }

    #region Implementation
    private void AssertSite(Site actual, List<Point> expectedPoints)
    {
      Assert.IsNotNull(actual);

      double expectedMaxLat;
      double expectedMinLat;
      double expectedMaxLon;
      double expectedMinLon;
      SiteBoundingBox(expectedPoints, out expectedMaxLat, out expectedMaxLon, out expectedMinLat, out expectedMinLon);

      Assert.AreEqual("Tulsa Test Site", actual.Name);
      Assert.AreEqual("Unit Test Site", actual.Description);
      Assert.AreEqual(true, actual.Visible);
      List<Point> actualPoints = actual.PolygonPoints.ToList();
      Assert.AreEqual(expectedPoints.Count, actualPoints.Count, "Wrong number of site points");
      foreach (Point p in expectedPoints)
      {
        Assert.IsNotNull(actualPoints.Find(delegate(Point pt) { return pt.x == p.x && pt.y == p.y; }), "Missing site point");
      }
      Assert.AreEqual(expectedMaxLat, actual.MaxLat);
      Assert.AreEqual(expectedMaxLon, actual.MaxLon);
      Assert.AreEqual(expectedMinLat, actual.MinLat);
      Assert.AreEqual(expectedMinLon, actual.MinLon);
      Assert.AreEqual(false, actual.Transparent);
      Assert.AreEqual(0x0000ff, actual.Colour);
    }

    private void SiteBoundingBox(List<Point> points, out double maxLat, out double maxLon, out double minLat, out double minLon)
    {
      maxLat = points[0].y;
      minLat = points[0].y;
      maxLon = points[0].x;
      minLon = points[0].x;
      for (int i = 1; i < points.Count; i++)
      {
        if (points[i].y < minLat) minLat = points[i].y;
        if (points[i].y > maxLat) maxLat = points[i].y;
        if (points[i].x < minLon) minLon = points[i].x;
        if (points[i].x > maxLon) maxLon = points[i].x;
      }
    }

    private List<Point> GetSitePoints()
    {
      List<Point> points = new List<Point>();
      Point point = new Point(36.1538889, -95.9925);
      points.Add(point);
      point = new Point(38.1538889, -90.9925);
      points.Add(point);
      point = new Point(42.25, -89.5);
      points.Add(point);
      point = new Point(40.154, -95.25);
      points.Add(point);
      return points;
    }

    #endregion


    [TestMethod()]
    [DatabaseTest]
    public void SiteAPI_IsPointInAmericasTest()
    {
      SiteAPI target = new SiteAPI(); 
      Assert.IsTrue(target.IsPointInAmericas(40, -105));
      Assert.IsFalse(target.IsPointInAmericas(0, 0));
      Assert.IsFalse(target.IsPointInAmericas(0, -22));
    }
  }
}
