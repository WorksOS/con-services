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
            new Transition(0.2, Color.DarkBlue),
            new Transition(0.1, Color.SkyBlue),
            new Transition(0.05, Color.Blue),
            new Transition(0.0, Color.Green),
            new Transition(-0.05, Color.Yellow),
            new Transition(-0.1, Color.Crimson),
            new Transition(-0.2, Color.Red)
        };

        public CutFillPalette() : base(Transitions)
        {
        }
    }
}
