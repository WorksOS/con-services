using Draw = System.Drawing;
using VSS.MasterData.Models;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw MDP data
  /// </summary>
  public class PassCountPalette : PaletteBase
  {
    private Draw.Color _targetPassCountColour = Draw.Color.Blue;

    private static Transition[] Transitions =
    {
      new Transition(1, Draw.Color.FromArgb(0x00A4A4)),
      new Transition(2, Draw.Color.Red),
      new Transition(3, Draw.Color.Aqua),
      new Transition(4, Draw.Color.FromArgb(0xAEAE00)),
      new Transition(5, Draw.Color.Lime),
      new Transition(6, Draw.Color.Fuchsia),
      new Transition(7, Draw.Color.FromArgb(0xB000B0)),
      new Transition(8, Draw.Color.DarkGray),
      new Transition(9, Draw.Color.FromArgb(0xACFDEB))
    };

    public static Draw.Color BelowPassTargetRangeColour = Transitions[0].Color;
    public static Draw.Color WithinPassTargetRangeColour = Transitions[1].Color;
    public static Draw.Color AbovePassTargetRangeColour = Transitions[2].Color;

    public PassCountPalette() : base(Transitions)
    {
      // ...
    }
  }
}

