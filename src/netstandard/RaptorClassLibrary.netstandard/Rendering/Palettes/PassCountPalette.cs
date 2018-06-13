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
    private const double TOLERANCE = 0.00001;

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

    public void InitSummatyColors(out Color above, out Color within, out Color below)
    {
      below = Transitions[0].Color;
      within = Transitions[1].Color;
      above = Transitions[2].Color;
    }

    public PassCountPalette() : base(Transitions)
    {
      // ...
    }
  }
}

