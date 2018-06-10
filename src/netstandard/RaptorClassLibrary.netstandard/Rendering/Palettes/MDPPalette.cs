using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw MDP data
  /// </summary>
  public class MDPPalette : PaletteBase
  {
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
    }
  }
}
