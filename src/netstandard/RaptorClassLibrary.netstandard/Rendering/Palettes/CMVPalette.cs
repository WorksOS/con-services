using System.Drawing;

namespace VSS.TRex.Rendering.Palettes
{
    /// <summary>
    /// Simple palette for rendering raw CMV data
    /// </summary>
    public class CMVPalette : PaletteBase
    {
        private static Transition[] Transitions =
        {
            new Transition(0, Color.Green),
            new Transition(20, Color.Yellow),
            new Transition(40, Color.Olive),
            new Transition(60, Color.Blue),
            new Transition(100, Color.SkyBlue)
        };

        public CMVPalette() : base(Transitions)
        {
        }
    }
}
