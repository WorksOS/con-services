using System.Drawing;

namespace VSS.TRex.Rendering.Palettes
{
  public class CCAPalette : PaletteBase
  {
    private static Transition[] Transitions =
    {
      new Transition(0, Color.Yellow),
      new Transition(1, Color.Red),
      new Transition(2, Color.Aqua),
      new Transition(3, Color.Lime),
      new Transition(4, ColorTranslator.FromHtml("#FF8080")),
      new Transition(5, ColorTranslator.FromHtml("#91C8FF")),
      new Transition(6, ColorTranslator.FromHtml("#ACFDEB")),
      new Transition(7, Color.Green),
      new Transition(8, ColorTranslator.FromHtml("#FFC0FF")),
      new Transition(9, ColorTranslator.FromHtml("#FFCB96")),
      new Transition(10, ColorTranslator.FromHtml("#6C8EB5")),
      new Transition(11, ColorTranslator.FromHtml("#80FFFF")),
      new Transition(12, ColorTranslator.FromHtml("#8080FF")),
      new Transition(13, ColorTranslator.FromHtml("#00FF80")),
      new Transition(14, ColorTranslator.FromHtml("#FF8000")),
      new Transition(15, ColorTranslator.FromHtml("#8000FF")),
      new Transition(16, Color.Teal),
      new Transition(17, ColorTranslator.FromHtml("#C0C0FF")),
      new Transition(18, ColorTranslator.FromHtml("#FF80FF")),
      new Transition(19, ColorTranslator.FromHtml("#80FF00")),
      new Transition(20, ColorTranslator.FromHtml("#80FF00")),
      new Transition(21, ColorTranslator.FromHtml("#80FF00")),
      new Transition(22, ColorTranslator.FromHtml("#80FF00")),
      new Transition(23, ColorTranslator.FromHtml("#80FF00")),
      new Transition(24, ColorTranslator.FromHtml("#80FF00")),
      new Transition(25, ColorTranslator.FromHtml("#80FF00")),
      new Transition(26, ColorTranslator.FromHtml("#80FF00")),
      new Transition(27, ColorTranslator.FromHtml("#80FF00")),
      new Transition(28, ColorTranslator.FromHtml("#80FF00")),
      new Transition(29, ColorTranslator.FromHtml("#80FF00")),
      new Transition(30, ColorTranslator.FromHtml("#80FF00")),
      new Transition(31, ColorTranslator.FromHtml("#80FF00")),
      new Transition(32, ColorTranslator.FromHtml("#80FF00")),
      new Transition(33, ColorTranslator.FromHtml("#80FF00")),
      new Transition(34, ColorTranslator.FromHtml("#80FF00")),
      new Transition(35, ColorTranslator.FromHtml("#80FF00")),
      new Transition(36, ColorTranslator.FromHtml("#80FF00")),
      new Transition(37, ColorTranslator.FromHtml("#80FF00")),
      new Transition(38, ColorTranslator.FromHtml("#80FF00")),
      new Transition(39, ColorTranslator.FromHtml("#80FF00")),
      new Transition(40, ColorTranslator.FromHtml("#80FF00")),
      new Transition(41, ColorTranslator.FromHtml("#80FF00")),
      new Transition(42, ColorTranslator.FromHtml("#80FF00")),
      new Transition(43, ColorTranslator.FromHtml("#80FF00")),
      new Transition(44, ColorTranslator.FromHtml("#80FF00")),
      new Transition(45, ColorTranslator.FromHtml("#80FF00")),
      new Transition(46, ColorTranslator.FromHtml("#80FF00")),
      new Transition(47, ColorTranslator.FromHtml("#80FF00")),
      new Transition(48, ColorTranslator.FromHtml("#80FF00")),
      new Transition(49, ColorTranslator.FromHtml("#80FF00"))
    };

    public CCAPalette() : base(Transitions)
    {
    }
  }
}
