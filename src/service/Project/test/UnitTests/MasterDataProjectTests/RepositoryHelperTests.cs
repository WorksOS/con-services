using VSS.MasterData.Repositories;
using Xunit;

namespace VSS.MasterData.ProjectTests
{
  public class RepositoryHelperTests   
  {
    private const string GeometryWKT =
      "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154))";

    [Fact]
    public void CanConvertEmptyWKTToSpatial()
    {
      var spatial = RepositoryHelper.WKTToSpatial(string.Empty);
      Assert.Equal("null", spatial);
    }

    [Fact]
    public void CanConvertNullWKTToSpatial()
    {
      var spatial = RepositoryHelper.WKTToSpatial(null);
      Assert.Equal("null", spatial);
    }

    [Fact]
    public void CanConvertValidWKTToSpatial()
    {
      var expected = $"ST_GeomFromText('{GeometryWKT}')";
      var spatial = RepositoryHelper.WKTToSpatial(GeometryWKT);
      Assert.Equal(expected, spatial);
    }
  }
}
