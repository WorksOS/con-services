using System.Drawing;

namespace VSS.TRex.Rendering.Palettes
{
  /// <summary>
  /// Simple palette for rendering raw Pass Count data
  /// </summary>
  public class PassCountPalette : PaletteBase
  {
    private static Transition[] Transitions =
    {
      new Transition(1, Color.DarkBlue),
      new Transition(2, Color.DodgerBlue),
      new Transition(3, Color.LightBlue),
      new Transition(4, Color.YellowGreen),
      new Transition(5, Color.DarkSeaGreen),
      new Transition(6, Color.DarkGreen),
      new Transition(7, Color.LightPink),
      new Transition(8, Color.RosyBrown),
      new Transition(9, Color.Brown)
    };

    public PassCountPalette() : base(Transitions)
    {
      // ...
    }
  }
}

