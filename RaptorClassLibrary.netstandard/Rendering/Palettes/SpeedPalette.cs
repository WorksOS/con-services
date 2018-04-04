using System.Drawing;

namespace VSS.VisionLink.Raptor.Rendering.Palettes
{
    /// <summary>
    /// Simple palette for rendering speed data
    /// </summary>
    public class SpeedPalette : PaletteBase
    {
        private static Transition[] Transitions =
        {
            new Transition(0, Color.Green),
            new Transition(500, Color.Yellow),
            new Transition(1000, Color.Olive),
            new Transition(1500, Color.Blue),
            new Transition(2500, Color.SkyBlue)
        };

        public SpeedPalette() : base(Transitions)
        {
        }
    }
}
