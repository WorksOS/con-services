using Draw = System.Drawing;

namespace VSS.TRex.Rendering.Palettes
{
  public class CCAPalette : PaletteBase
  {
    private static Transition[] Transitions =
    {
      new Transition(0, Draw.Color.Yellow),
      new Transition(1, Draw.Color.Red),
      new Transition(2, Draw.Color.Aqua),
      new Transition(3, Draw.Color.Lime),
      new Transition(4, Draw.ColorTranslator.FromHtml("#FF8080")),
      new Transition(5, Draw.ColorTranslator.FromHtml("#91C8FF")),
      new Transition(6, Draw.ColorTranslator.FromHtml("#ACFDEB")),
      new Transition(7, Draw.Color.Green),
      new Transition(8, Draw.ColorTranslator.FromHtml("#FFC0FF")),
      new Transition(9, Draw.ColorTranslator.FromHtml("#FFCB96")),
      new Transition(10, Draw.ColorTranslator.FromHtml("#6C8EB5")),
      new Transition(11, Draw.ColorTranslator.FromHtml("#80FFFF")),
      new Transition(12, Draw.ColorTranslator.FromHtml("#8080FF")),
      new Transition(13, Draw.ColorTranslator.FromHtml("#00FF80")),
      new Transition(14, Draw.ColorTranslator.FromHtml("#FF8000")),
      new Transition(15, Draw.ColorTranslator.FromHtml("#8000FF")),
      new Transition(16, Draw.Color.Teal),
      new Transition(17, Draw.ColorTranslator.FromHtml("#C0C0FF")),
      new Transition(18, Draw.ColorTranslator.FromHtml("#FF80FF")),
      new Transition(19, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(20, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(21, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(22, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(23, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(24, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(25, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(26, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(27, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(28, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(29, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(30, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(31, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(32, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(33, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(34, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(35, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(36, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(37, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(38, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(39, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(40, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(41, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(42, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(43, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(44, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(45, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(46, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(47, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(48, Draw.ColorTranslator.FromHtml("#80FF00")),
      new Transition(49, Draw.ColorTranslator.FromHtml("#80FF00"))
    };

    public CCAPalette() : base(Transitions)
    {
    }
  }
}
