using System.Drawing;

namespace VSS.VisionLink.Raptor.Rendering.Palettes
{
    /// <summary>
    /// Simple palette for rendering speed data
    /// </summary>
    public class CutFillPalette : PaletteBase
    {
        private static Transition[] Transitions =
        {
            new Transition(-10000.0, Color.Red),
            new Transition(-2.0, Color.Red),
            new Transition(-1.0, Color.Yellow),
            new Transition(-0.1, Color.Green),
            new Transition(0.1, Color.Blue),
            new Transition(1.0, Color.SkyBlue),
            new Transition(2.0, Color.DarkBlue),
            new Transition(10000, Color.DarkBlue)
        };

        public CutFillPalette() : base(Transitions)
        {
        }
    }
}
