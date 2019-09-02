using System;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Exports.Patches.GridFabric.PatchRequestWithColors;
using VSS.TRex.Filters;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Patches
{
  public class ToFromBinary_PatchRequestWithColorsArgument : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_PatchRequestWithColorsArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<PatchRequestWithColorsArgument>("Empty PatchRequestWithColorsArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_PatchRequestWithColorsArgument()
    {
      var argument = new PatchRequestWithColorsArgument()
      {
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        DataPatchNumber = 0,
        DataPatchSize = 10,
        Mode = DisplayMode.CCV,
        RenderValuesToColours = true,
        ColourPalette = TileRenderRequestArgumentPaletteFactory.GetPalette(DisplayMode.CCV)
    };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom PatchRequestWithColorsArgument not same after round trip serialisation");
    }
  }
}
