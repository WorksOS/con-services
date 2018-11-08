using ASNodeRaptorReports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;

namespace VSS.Productivity3D.WebApiTests.Report.Models
{
  [TestClass]
  public class StationOffsetRowTests
  {
    [TestMethod]
    public void Constructor_should_return_fully_populated_object()
    {
      var stationOffset = new TStationOffset
        {
          Northing = 12.34,
          Easting = 56.78,
          Elevation = 90.12,
          CutFill = 34.56,
          CMV = 78,
          MDP = 90,
          PassCount = 12345,
          Temperature = 21.43,
          Position = 54.65,
          Station = 87.09
      };

      var request = CompactionReportStationOffsetRequest.CreateRequest(
      0, null, 0, null, true, true, true, true, true, true, null, null, 0, 0, 0, null, null, null
      );

      var result = StationOffsetRow.CreateRow(stationOffset, request);

      Assert.AreEqual(stationOffset.Northing, result.Northing);
      Assert.AreEqual(stationOffset.Easting, result.Easting);
      Assert.AreEqual(stationOffset.Elevation, result.Elevation);
      Assert.AreEqual(stationOffset.CutFill, result.CutFill);
      Assert.AreEqual((double)stationOffset.CMV / 10, result.CMV);
      Assert.AreEqual((double)stationOffset.MDP / 10, result.MDP);
      Assert.AreEqual(stationOffset.PassCount, result.PassCount);
      Assert.AreEqual((double)stationOffset.Temperature / 10, result.Temperature);
      Assert.AreEqual(stationOffset.Position, result.Offset);
      Assert.AreEqual(stationOffset.Station, result.Station);

      Assert.IsTrue(result.ElevationReport);
      Assert.IsTrue(result.CutFillReport);
      Assert.IsTrue(result.CMVReport);
      Assert.IsTrue(result.MDPReport);
      Assert.IsTrue(result.PassCountReport);
      Assert.IsTrue(result.TemperatureReport);
    }
  }
}
