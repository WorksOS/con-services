using ASNodeRaptorReports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;

namespace VSS.Productivity3D.WebApiTests.Report.Models
{
  [TestClass]
  public class GridRowTests
  {
    [TestMethod]
    public void Constructor_should_return_fully_populated_object()
    {
      var stationOffset = new TGridRow
      {
        Northing = 12.34,
        Easting = 56.78,
        Elevation = 90.12,
        CutFill = 34.56,
        CMV = 78,
        MDP = 90,
        PassCount = 12345,
        Temperature = 21
      };

      var result = GridRow.CreateRow(stationOffset);

      Assert.AreEqual(stationOffset.Northing, result.Northing);
      Assert.AreEqual(stationOffset.Easting, result.Easting);
      Assert.AreEqual(stationOffset.Elevation, result.Elevation);
      Assert.AreEqual(stationOffset.CutFill, result.CutFill);
      Assert.AreEqual(stationOffset.CMV, result.CMV);
      Assert.AreEqual(stationOffset.MDP, result.MDP);
      Assert.AreEqual(stationOffset.PassCount, result.PassCount);
      Assert.AreEqual(stationOffset.Temperature, result.Temperature);
    }
  }
}