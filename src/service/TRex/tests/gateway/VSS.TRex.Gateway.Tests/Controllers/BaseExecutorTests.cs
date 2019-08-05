using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Gateway.Common.Executors;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers
{
  public class BaseExecutorTests : BaseExecutor //derive from this so can call protected method that is being tested
  {
    [Fact]
    public void CanConvertLiftWithNoSettingsOrFilterLayerMethod()
    {
      var liftParams = ConvertLift(null, null);
      Assert.NotNull(liftParams);
      Assert.Equal(Types.LiftDetectionType.None, liftParams.LiftDetectionType);
    }

    [Theory]
    [InlineData(LiftDetectionType.None)]
    [InlineData(LiftDetectionType.Automatic)]
    [InlineData(LiftDetectionType.AutoMapReset)]
    [InlineData(LiftDetectionType.MapReset)]
    [InlineData(LiftDetectionType.Tagfile)]
    public void CanConvertLiftWithSpecifiedSettingsAndNoFilterLayerMethod(LiftDetectionType layerType)
    {
      var liftSettings = new LiftSettings(false, false, SummaryType.Compaction, SummaryType.Compaction, 
        0, layerType, LiftThicknessType.Compacted, null, false, 0, 
        false, 0, 0);
      var liftParams = ConvertLift(liftSettings, null);
      Assert.NotNull(liftParams);
      Assert.Equal((Types.LiftDetectionType)layerType, liftParams.LiftDetectionType);
    }

    [Theory]
    [InlineData(FilterLayerMethod.Invalid)]
    [InlineData(FilterLayerMethod.AutoMapReset)]
    [InlineData(FilterLayerMethod.Automatic)]
    [InlineData(FilterLayerMethod.MapReset)]
    [InlineData(FilterLayerMethod.None)]
    [InlineData(FilterLayerMethod.OffsetFromBench)]
    [InlineData(FilterLayerMethod.OffsetFromDesign)]
    [InlineData(FilterLayerMethod.OffsetFromProfile)]
    [InlineData(FilterLayerMethod.TagfileLayerNumber)]
    public void CanConvertLiftWithNoSettingsAndSpecifiedFilterLayerMethod(FilterLayerMethod layerMethod)
    {
      var liftParams = ConvertLift(null, layerMethod);
      Assert.NotNull(liftParams);
      var expected = mapping[layerMethod];
      Assert.Equal(expected, liftParams.LiftDetectionType);
    }

    [Theory]
    [InlineData(FilterLayerMethod.Invalid)]
    [InlineData(FilterLayerMethod.AutoMapReset)]
    [InlineData(FilterLayerMethod.Automatic)]
    [InlineData(FilterLayerMethod.MapReset)]
    [InlineData(FilterLayerMethod.None)]
    [InlineData(FilterLayerMethod.OffsetFromBench)]
    [InlineData(FilterLayerMethod.OffsetFromDesign)]
    [InlineData(FilterLayerMethod.OffsetFromProfile)]
    [InlineData(FilterLayerMethod.TagfileLayerNumber)]
    public void CanConvertLiftWithDefaultSettingsAndSpecifiedFilterLayerMethod(FilterLayerMethod layerMethod)
    {
      var liftSettings = new LiftSettings();
      var liftParams = ConvertLift(liftSettings, layerMethod);
      Assert.NotNull(liftParams);
      var expected = mapping[layerMethod];
      Assert.Equal(expected, liftParams.LiftDetectionType);
    }

    [Theory]
    [InlineData(FilterLayerMethod.Invalid, LiftDetectionType.Tagfile)]
    [InlineData(FilterLayerMethod.AutoMapReset, LiftDetectionType.None)]
    [InlineData(FilterLayerMethod.Automatic, LiftDetectionType.None)]
    [InlineData(FilterLayerMethod.MapReset, LiftDetectionType.Automatic)]
    [InlineData(FilterLayerMethod.None, LiftDetectionType.Automatic)]
    [InlineData(FilterLayerMethod.OffsetFromBench, LiftDetectionType.Automatic)]
    [InlineData(FilterLayerMethod.OffsetFromDesign, LiftDetectionType.AutoMapReset)]
    [InlineData(FilterLayerMethod.OffsetFromProfile, LiftDetectionType.AutoMapReset)]
    [InlineData(FilterLayerMethod.TagfileLayerNumber, LiftDetectionType.AutoMapReset)]
    [InlineData(FilterLayerMethod.Invalid, LiftDetectionType.MapReset)]
    [InlineData(FilterLayerMethod.AutoMapReset, LiftDetectionType.MapReset)]
    [InlineData(FilterLayerMethod.Automatic, LiftDetectionType.MapReset)]
    [InlineData(FilterLayerMethod.MapReset, LiftDetectionType.Tagfile)]
    [InlineData(FilterLayerMethod.None, LiftDetectionType.MapReset)]
    [InlineData(FilterLayerMethod.OffsetFromBench, LiftDetectionType.Tagfile)]
    [InlineData(FilterLayerMethod.OffsetFromDesign, LiftDetectionType.Tagfile)]
    [InlineData(FilterLayerMethod.OffsetFromProfile, LiftDetectionType.Automatic)]
    [InlineData(FilterLayerMethod.TagfileLayerNumber, LiftDetectionType.None)]
    public void CanConvertLiftWithSpecifiedSettingsAndSpecifiedFilterLayerMethod(FilterLayerMethod layerMethod, LiftDetectionType layerType)
    {
      var liftSettings = new LiftSettings(false, false, SummaryType.Compaction, SummaryType.Compaction,
        0, layerType, LiftThicknessType.Compacted, null, false, 0,
        false, 0, 0);
      var liftParams = ConvertLift(liftSettings, layerMethod);
      Assert.NotNull(liftParams);
      var expected = layerMethod == FilterLayerMethod.Invalid ? (Types.LiftDetectionType)layerType : mapping[layerMethod];
      Assert.Equal(expected, liftParams.LiftDetectionType);
    }

    private readonly Dictionary<FilterLayerMethod, Types.LiftDetectionType> mapping = new Dictionary<FilterLayerMethod, Types.LiftDetectionType>
    {
      {FilterLayerMethod.AutoMapReset, Types.LiftDetectionType.AutoMapReset},
      {FilterLayerMethod.Automatic, Types.LiftDetectionType.Automatic},
      {FilterLayerMethod.MapReset, Types.LiftDetectionType.MapReset},
      {FilterLayerMethod.OffsetFromDesign, Types.LiftDetectionType.None},
      {FilterLayerMethod.OffsetFromBench, Types.LiftDetectionType.None},
      {FilterLayerMethod.OffsetFromProfile, Types.LiftDetectionType.None},
      {FilterLayerMethod.TagfileLayerNumber, Types.LiftDetectionType.Tagfile},
      {FilterLayerMethod.Invalid, Types.LiftDetectionType.None},
      {FilterLayerMethod.None, Types.LiftDetectionType.None}
    };
  }
}
