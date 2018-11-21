using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;

namespace VSS.Productivity3D.WebApiTests.Compaction.Helpers
{
  [TestClass]
  public class CompactionSettingsManagerTests
  {
    [TestMethod]
    [DataRow(DisplayMode.Design3D)]
    [DataRow(DisplayMode.Height)]
    public void CompactionPalette_Should_return_null_When_ElevationExtents_is_null(DisplayMode displayMode)
    {
      var compactionSettingsManager = new CompactionSettingsManager();

      var palette = compactionSettingsManager.CompactionPalette(displayMode, null, null, null);

      Assert.IsNull(palette);
    }

    [TestMethod]
    [DataRow(DisplayMode.Design3D)]
    [DataRow(DisplayMode.Height)]
    public void CompactionPalette_Should_return_default_Evelvation_palette(DisplayMode displayMode)
    {
      var elevExtents = ElevationStatisticsResult.CreateElevationStatisticsResult(
        BoundingBox3DGrid.CreatBoundingBox3DGrid(100.0, 100.0, 100.0, 100.0, 100.0, 100.0),
        100.0,
        100.0,
        1000.0);

      var palette = new CompactionSettingsManager().CompactionPalette(
        displayMode,
        elevExtents,
        null,
        CompactionProjectSettingsColors.Create());

      Assert.IsNotNull(palette);
      Assert.AreEqual(31, palette.Count);
    }
  }
}
