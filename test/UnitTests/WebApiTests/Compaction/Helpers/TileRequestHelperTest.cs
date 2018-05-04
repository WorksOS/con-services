using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using WGSPoint = VSS.Productivity3D.Common.Models.WGSPoint;

namespace VSS.Productivity3D.WebApiTests.Compaction.Helpers
{
  [TestClass]
  public class TileRequestHelperTest
  {
    [Ignore("Under development as part of story #68588")]
    [TestClass]
    public class CreateTileRequestTests : TileRequestHelperTest
    {
      private readonly DesignDescriptor designDescriptor = DesignDescriptor.CreateDesignDescriptor(1, null, 0.0);

      [TestMethod]
      public void Should_set_FilterlayoutMethod()
      {
        var filter = new Filter();

        var polygonLonLat = new List<WGSPoint>
        {
          WGSPoint.CreatePoint(1, 1),
          WGSPoint.CreatePoint(2, 2),
          WGSPoint.CreatePoint(3, 3)
        };

        var filterResult = FilterResult.CreateFilter(filter, polygonLonLat, null, FilterLayerMethod.None, null, false, designDescriptor);

        var requestHelper = new TileRequestHelper();
        var settingsManager = new CompactionSettingsManager();

        var compactionProjectSettings = CompactionProjectSettings.CreateProjectSettings();

        requestHelper.Initialize(null, null, null, settingsManager, 0, compactionProjectSettings, null, null, filterResult, designDescriptor);

        var tileRequestResult = requestHelper.CreateTileRequest(DisplayMode.CutFill, 0, 0, null, null);

        Assert.AreEqual(FilterLayerMethod.None, tileRequestResult.FilterLayerMethod);
      }
    }
  }
}
