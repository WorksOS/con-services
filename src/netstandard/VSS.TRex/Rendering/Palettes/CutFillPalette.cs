using Draw = System.Drawing;

namespace VSS.TRex.Rendering.Palettes
{
    /// <summary>
    /// Simple palette for rendering speed data
    /// </summary>
    public class CutFillPalette : PaletteBase
    {
        private static Transition[] Transitions =
        {
            new Transition(-10000.0, Draw.Color.Red),
            new Transition(-2.0, Draw.Color.Red),
            new Transition(-1.0, Draw.Color.Yellow),
            new Transition(-0.1, Draw.Color.Green),
            new Transition(0.1, Draw.Color.Blue),
            new Transition(1.0, Draw.Color.SkyBlue),
            new Transition(2.0, Draw.Color.DarkBlue),
            new Transition(10000, Draw.Color.DarkBlue)
        };

        public CutFillPalette() : base(Transitions)
        {
        }
    }
}
