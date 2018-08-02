using Draw = System.Drawing;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw MDP data
  /// </summary>
  public class MDPPalette : PaletteBase
  {
    public bool DisplayTargetMDPColourInPVM { get; set; }

    private MDPRangePercentageRecord _mdpPercentageRange = new MDPRangePercentageRecord(80, 120);
    private Draw.Color _targetMDPColour = Draw.Color.Blue;

    private double _minTarget;
    private double _maxTarget;

    private static Transition[] Transitions =
    {
      new Transition(0, Draw.Color.Yellow),
      new Transition(1000, Draw.Color.Red),
      new Transition(1200, Draw.Color.Aqua),
      new Transition(1400, Draw.Color.Lime),
      new Transition(1600, Draw.Color.FromArgb(0xFF8080))
    };

    public MDPPalette() : base(Transitions)
    {
      _minTarget = _mdpPercentageRange.Min / 100;
      _maxTarget = _mdpPercentageRange.Max / 100;
    }

    public Draw.Color ChooseColour(double value, double targetValue)
    {
      // Check to see if the value is in the target range and use the target MDP colour
      // if it is. MDPRange holds a min/max percentage of target MDP...
      if (DisplayTargetMDPColourInPVM && (value >= targetValue * _minTarget && value <= targetValue * _maxTarget))
        return _targetMDPColour;

      return ChooseColour(value);
    }
  }
}
