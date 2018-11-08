using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Velociraptor.PDSInterface;

namespace VSS.Productivity3D.WebApiTests.Compaction.Helpers
{
  [TestClass]
  public class ProfilesHelperTests
  {
    [TestMethod]
    public void CelLGapExists_Should_handle_null_prevCell()
    {
      var currCell = new ProfileCell();

      Assert.IsFalse(ProfilesHelper.CellGapExists(null, currCell, out double prevStationIntercept));
      Assert.AreEqual(0.0, prevStationIntercept);
    }

    [TestMethod]
    [DataRow(0.0, 0.0, 0.0, false, 0)]
    [DataRow(0.001, 0.0, 0.001, false, 0.001)]
    [DataRow(0.002, 0.001, 0.001, true, 0.003)]
    [DataRow(0.001, 0.001, 0.001, false, 0.002)]
    [DataRow(0.003, 0.001, 0.001, true, 0.004)]
    public void CellGapExists(double prevStation, double prevInterceptLength, double currStation, bool expectedResult, double expectedPrevStationIntercept)
    {
      var prevCell = new ProfileCell
      {
        station = prevStation,
        interceptLength = prevInterceptLength
      };

      var currCell = new ProfileCell
      {
        station = currStation
      };

      Assert.AreEqual(expectedResult, ProfilesHelper.CellGapExists(prevCell, currCell, out double prevStationIntercept));
      Assert.AreEqual(expectedPrevStationIntercept, prevStationIntercept);
    }
  }
}
