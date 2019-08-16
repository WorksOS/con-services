using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Productivity3D.Models.Compaction;

namespace VSS.Productivity3D.WebApiTests.Compaction.Helpers
{
  [TestClass]
  public class TileRequestHelperTest
  {
    /// <summary>
    /// Does not yet test setting of the LiftSettings or Palette settings.
    /// </summary>
    [TestClass]
    public class CreateTileRequestTests : TileRequestHelperTest
    {
      private readonly DesignDescriptor designDescriptor = new DesignDescriptor(1, null, 0.0);
      private readonly DesignDescriptor volumeDescriptor = new DesignDescriptor(2, null, 0.0);

      private readonly List<WGSPoint> polygonLonLat = new List<WGSPoint>
      {
        new WGSPoint(1, 1),
        new WGSPoint(2, 2),
        new WGSPoint(3, 3)
      };

      private readonly TileRequestHelper requestHelper = new TileRequestHelper();
      private readonly CompactionSettingsManager settingsManager = new CompactionSettingsManager();
      private readonly CompactionProjectSettings compactionProjectSettings = CompactionProjectSettings.CreateProjectSettings();
      private readonly CompactionProjectSettingsColors compactionProjectSettingsColors = CompactionProjectSettingsColors.Create();

      private FilterResult InitRequestHelper(FilterLayerMethod layerMethod = FilterLayerMethod.None)
      {    
       var filterResult = new FilterResult(polygonLL:polygonLonLat, layerType:layerMethod, returnEarliest:false, designFile:designDescriptor);

        requestHelper.Initialize(null, null, null, settingsManager, null, 0, compactionProjectSettings, compactionProjectSettingsColors, null, filterResult, designDescriptor);
        return filterResult;
      }

      [TestMethod]
      [DataRow(FilterLayerMethod.None)]
      [DataRow(FilterLayerMethod.Automatic)]
      [DataRow(FilterLayerMethod.AutoMapReset)]
      public void Should_set_FilterlayoutMethod(FilterLayerMethod filterLayerMethod)
      {
        InitRequestHelper(filterLayerMethod);

        var tileRequestResult = requestHelper.CreateTileRequest(DisplayMode.CutFill, 0, 0, null, null);

        Assert.AreEqual(filterLayerMethod, tileRequestResult.FilterLayerMethod);
      }

      [TestMethod]
      [DataRow(DisplayMode.CCASummary)]
      [DataRow(DisplayMode.CCVPercent)]
      [DataRow(DisplayMode.CCVPercentSummary)]
      [DataRow(DisplayMode.CMVChange)]
      [DataRow(DisplayMode.PassCount)]
      public void Should_use_default_DesignDescriptor_and_Filter_When_not_DisplayModeCutFill(DisplayMode displayMode)
      {
        var filterResult = InitRequestHelper();

        var tileRequestResult = requestHelper.CreateTileRequest(displayMode, 0, 0, null, null);

        Assert.AreEqual(designDescriptor, tileRequestResult.DesignDescriptor, "DesignDescriptor should have default non null value.");
        Assert.AreEqual(filterResult, tileRequestResult.Filter1, "Filter1 should have default non null value.");
        Assert.IsNull(tileRequestResult.Filter2);
      }

      [TestMethod]
      [DataRow(VolumeCalcType.None)]
      [DataRow(VolumeCalcType.GroundToGround)]
      public void Should_use_default_Design(VolumeCalcType volumeCalcType)
      {
        InitRequestHelper();
        requestHelper.SetVolumeCalcType(volumeCalcType);

        var tileRequestResult = requestHelper.CreateTileRequest(DisplayMode.CutFill, 0, 0, null, null);

        Assert.AreEqual(designDescriptor, tileRequestResult.DesignDescriptor);
      }

      [TestMethod]
      [DataRow(VolumeCalcType.DesignToGround)]
      [DataRow(VolumeCalcType.GroundToDesign)]
      public void Should_set_Design_from_VolumeDescriptor_When_VolCalType_is_valid(VolumeCalcType volumeCalcType)
      {
        InitRequestHelper();
        requestHelper.SetVolumeCalcType(volumeCalcType);
        requestHelper.SetVolumeDesign(volumeDescriptor);

        var tileRequestResult = requestHelper.CreateTileRequest(DisplayMode.CutFill, 0, 0, null, null);

        Assert.AreEqual(volumeDescriptor, tileRequestResult.DesignDescriptor);
      }

      [TestMethod]
      [DataRow(VolumeCalcType.DesignToGround)]
      [DataRow(VolumeCalcType.GroundToDesign)]
      public void Should_not_set_topFilter_When_VolCalType_is_DesignToGround_or_GroundToDesign(VolumeCalcType volumeCalcType)
      {
        var baseFilter = InitRequestHelper();
        requestHelper.SetVolumeCalcType(volumeCalcType);
        requestHelper.SetBaseFilter(baseFilter);

        var tileRequestResult = requestHelper.CreateTileRequest(DisplayMode.CutFill, 0, 0, null, null);

        Assert.AreEqual(baseFilter, tileRequestResult.Filter1);
        Assert.IsNull(tileRequestResult.Filter2);
      }

      [TestMethod]
      [DataRow(VolumeCalcType.DesignToGround)]
      [DataRow(VolumeCalcType.GroundToDesign)]
      public void Should_set_Filter1_from_topFilter_When_VolCalType_is_valid(VolumeCalcType volumeCalcType)
      {
        var topFilter = InitRequestHelper();
        requestHelper.SetVolumeCalcType(volumeCalcType);
        requestHelper.SetTopFilter(topFilter);

        var tileRequestResult = requestHelper.CreateTileRequest(DisplayMode.CutFill, 0, 0, null, null);

        Assert.AreEqual(topFilter, tileRequestResult.Filter1);
        Assert.IsNull(tileRequestResult.Filter2);
      }

      [TestMethod]
      public void Should_set_Filter1_from_topFilter_When_VolCalType_valid()
      {
        var baseFilter = InitRequestHelper();
        requestHelper.SetVolumeCalcType(VolumeCalcType.GroundToGround);
        requestHelper.SetBaseFilter(baseFilter);

        var topFilter = new FilterResult(polygonLL: polygonLonLat, layerType: FilterLayerMethod.None, returnEarliest: false, designFile: designDescriptor); 
        requestHelper.SetTopFilter(topFilter);

        var tileRequestResult = requestHelper.CreateTileRequest(DisplayMode.CutFill, 0, 0, null, null);

        Assert.AreEqual(baseFilter, tileRequestResult.Filter1);
        Assert.AreEqual(topFilter, tileRequestResult.Filter2);
      }
    }
  }
}
