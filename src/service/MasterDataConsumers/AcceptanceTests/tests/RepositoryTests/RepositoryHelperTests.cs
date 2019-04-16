using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Repositories;

namespace RepositoryTests
{
  [TestClass]
  public class RepositoryHelperTests   
  {
    private const string GeometryWKT =
      "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154))";

    [TestMethod]
    public void CanConvertEmptyWKTToSpatial()
    {
      var spatial = RepositoryHelper.WKTToSpatial(string.Empty);
      Assert.AreEqual("null", spatial);
    }

    [TestMethod]
    public void CanConvertNullWKTToSpatial()
    {
      var spatial = RepositoryHelper.WKTToSpatial(null);
      Assert.AreEqual("null", spatial);
    }

    [TestMethod]
    public void CanConvertValidWKTToSpatial()
    {
      var expected = $"ST_GeomFromText('{GeometryWKT}')";
      var spatial = RepositoryHelper.WKTToSpatial(GeometryWKT);
      Assert.AreEqual(expected, spatial);
    }

    [TestMethod]
    public void PolygonWKTOldBoundaryClosed()
    {
      var oldBoundary =
        "172.68231141046,-43.6277661929154;172.692096108947,-43.6213045879588;172.701537484681,-43.6285117180247;172.68231141046,-43.6277661929154";
      var wkt = RepositoryHelper.GetPolygonWKT(oldBoundary);
      var expected =
        "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.68231141046 -43.6277661929154))";
      Assert.AreEqual(expected, wkt);
    }

    [TestMethod]
    public void PolygonWKTOldBoundaryNotClosed()
    {
      var oldBoundary =
        "172.68231141046,-43.6277661929154;172.692096108947,-43.6213045879588;172.701537484681,-43.6285117180247";
      var wkt = RepositoryHelper.GetPolygonWKT(oldBoundary);
      var expected =
        "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.68231141046 -43.6277661929154))";
      Assert.AreEqual(expected, wkt);
    }

    [TestMethod]
    public void PolygonWKTNewBoundaryClosed()
    {
      var wkt = RepositoryHelper.GetPolygonWKT(GeometryWKT);
      Assert.AreEqual(GeometryWKT, wkt);
    }

    [TestMethod]
    public void PolygonWKTNewBoundaryNotClosed()
    {
      var boundary = GeometryWKT.Substring(0, GeometryWKT.LastIndexOf(',')) + "))";
      var wkt = RepositoryHelper.GetPolygonWKT(boundary);
      Assert.AreEqual(GeometryWKT, wkt);
    }
  }
}
