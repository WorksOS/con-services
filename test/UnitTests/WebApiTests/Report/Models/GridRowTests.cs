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
      var gridRow = new TGridRow
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

      var request = CompactionReportGridRequest.CreateCompactionReportGridRequest(0, null, 0, null, true, true, true, true, true, true, null, null, GridReportOption.Unused, 0, 0, 0, 0, 0);
      var result = GridRow.CreateRow(gridRow, request);

      Assert.AreEqual(gridRow.Northing, result.Northing);
      Assert.AreEqual(gridRow.Easting, result.Easting);
      Assert.AreEqual(gridRow.Elevation, result.Elevation);
      Assert.AreEqual(gridRow.CutFill, result.CutFill);
      Assert.AreEqual((double)gridRow.CMV / 10, result.CMV);
      Assert.AreEqual((double)gridRow.MDP / 10, result.MDP);
      Assert.AreEqual(gridRow.PassCount, result.PassCount);
      Assert.AreEqual((double)gridRow.Temperature / 10, result.Temperature);

      Assert.IsTrue(result.ElevationReport);
      Assert.IsTrue(result.CutFillReport);
      Assert.IsTrue(result.CMVReport);
      Assert.IsTrue(result.MDPReport);
      Assert.IsTrue(result.PassCountReport);
      Assert.IsTrue(result.TemperatureReport);
    }
  }
}
