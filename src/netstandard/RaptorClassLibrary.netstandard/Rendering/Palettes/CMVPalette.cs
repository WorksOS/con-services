using System.Drawing;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw CMV data
  /// </summary>
  public class CMVPalette : PaletteBase
  {
    public bool DisplayTargetCCVColourInPVM { get; set; }
    public bool DisplayDecoupledColourInPVM { get; set; }

    private CMVRangePercentageRecord _cmvPercentageRange = new CMVRangePercentageRecord(30, 80);
    private Color _targetCCVColour = Color.Blue;

    private static Transition[] Transitions =
    {
      new Transition(0, Color.Green),
      new Transition(20, Color.Yellow),
      new Transition(40, Color.Olive),
      new Transition(60, Color.Blue),
      new Transition(100, Color.SkyBlue)
    };

    public CMVPalette() : base(Transitions)
    {
    }

    public Color ChooseColour(double value, double targetValue)
    {
      // Check to see if the value is in the target range and use the target CMV colour
      // if it is. CCVRange holds a min/max percentage of target CMV...
      if (DisplayTargetCCVColourInPVM && (value >= targetValue * (_cmvPercentageRange.Min / 100) && value <= targetValue * (_cmvPercentageRange.Max / 100)))
        return _targetCCVColour;

      return ChooseColour(value);
    }
  }
}
