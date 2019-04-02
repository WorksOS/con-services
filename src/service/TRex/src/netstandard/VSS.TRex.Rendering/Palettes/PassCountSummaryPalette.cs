using System.Drawing;
using VSS.TRex.Common.Records;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw Pass Count summary data
  /// </summary>
  public class PassCountSummaryPalette : PaletteBase
  {
    /// <summary>
    /// The color, which Pass Count summary data displayed in on a plan view map, where pass count values are greater than target range.
    /// </summary>
    public static Color AbovePassTargetRangeColour = Color.Red;

    /// <summary>
    /// The color, which Pass Count summary data displayed in on a plan view map, where pass count values are within target range.
    /// </summary>
    public static Color WithinPassTargetRangeColour = Color.YellowGreen;

    /// <summary>
    /// The color, which Pass Count summary data displayed in on a plan view map, where pass count values are less than target range.
    /// </summary>
    public static Color BelowPassTargetRangeColour = Color.DodgerBlue;

    public PassCountSummaryPalette() : base(null)
    {
      // ...
    }

    public Color ChooseColour(ushort measuredPassCount, PassCountRangeRecord passTargetRange)
    {
      if (measuredPassCount < passTargetRange.Min)
        return BelowPassTargetRangeColour;

      if (measuredPassCount > passTargetRange.Max)
        return AbovePassTargetRangeColour;

      return WithinPassTargetRangeColour;
    }
  }
}
