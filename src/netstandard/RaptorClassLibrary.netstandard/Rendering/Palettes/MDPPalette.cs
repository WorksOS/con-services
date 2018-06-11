using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw MDP data
  /// </summary>
  public class MDPPalette : PaletteBase
  {
    public bool DisplayTargetMDPColourInPVM { get; set; }

    private MDPRangePercentageRecord _cmvPercentageRange = new MDPRangePercentageRecord(80, 120);
    private Color _targetMDPColour = Color.Blue;

    private static Transition[] Transitions =
    {
      new Transition(0, Color.Yellow),
      new Transition(1000, Color.Red),
      new Transition(1200, Color.Aqua),
      new Transition(1400, Color.Lime),
      new Transition(1600, Color.FromArgb(0xFF8080))
    };

    public MDPPalette() : base(Transitions)
    {
      // ...
    }

    public Color ChooseColour(double value, double targetValue)
    {
      // Check to see if the value is in the target range and use the target MDP colour
      // if it is. MDPRange holds a min/max percentage of target MDP...
      if (DisplayTargetMDPColourInPVM && (value >= targetValue * (_cmvPercentageRange.Min / 100) && value <= targetValue * (_cmvPercentageRange.Max / 100)))
        return _targetMDPColour;

      return ChooseColour(value);
    }
  }
}
