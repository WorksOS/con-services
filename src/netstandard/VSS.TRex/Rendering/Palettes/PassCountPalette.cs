using System;
using System.Drawing;
using VSS.MasterData.Models;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw MDP data
  /// </summary>
  public class PassCountPalette : PaletteBase
  {
    private Color _targetPassCountColour = Color.Blue;

    private static Transition[] Transitions =
    {
      new Transition(1, Color.FromArgb(0x00A4A4)),
      new Transition(2, Color.Red),
      new Transition(3, Color.Aqua),
      new Transition(4, Color.FromArgb(0xAEAE00)),
      new Transition(5, Color.Lime),
      new Transition(6, Color.Fuchsia),
      new Transition(7, Color.FromArgb(0xB000B0)),
      new Transition(8, Color.DarkGray),
      new Transition(9, Color.FromArgb(0xACFDEB))
    };

    public static Color BelowPassTargetRangeColour = Transitions[0].Color;
    public static Color WithinPassTargetRangeColour = Transitions[1].Color;
    public static Color AbovePassTargetRangeColour = Transitions[2].Color;

    public PassCountPalette() : base(Transitions)
    {
      // ...
    }
  }
}

