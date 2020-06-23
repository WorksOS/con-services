using System.Drawing;

namespace VSS.TRex.Rendering.Palettes
{
    /// <summary>
    /// Simple palette for rendering speed data
    /// </summary>
    public class CutFillPalette : PaletteBase
    {
        private static Transition[] Transitions =
        {
            new Transition(10000, Color.DarkBlue),
            new Transition(2.0, Color.DarkBlue),
            new Transition(1.0, Color.SkyBlue),
            new Transition(0.1, Color.Blue),
            new Transition(0.0, Color.Green),
            new Transition(-0.1, Color.Green),
            new Transition(-1.0, Color.Yellow),
            new Transition(-2.0, Color.Red),
            new Transition(-10000.0, Color.Red)
        };

        public CutFillPalette() : base(Transitions)
        {
        }
    }
}
