using Draw = System.Drawing;

namespace VSS.TRex.Rendering.Palettes
{
    /// <summary>
    /// Simple palette for rendering speed data
    /// </summary>
    public class SpeedPalette : PaletteBase
    {
        private static Transition[] Transitions =
        {
            new Transition(0, Draw.Color.Green),
            new Transition(500, Draw.Color.Yellow),
            new Transition(1000, Draw.Color.Olive),
            new Transition(1500, Draw.Color.Blue),
            new Transition(2500, Draw.Color.SkyBlue)
        };

      
        public SpeedPalette() : base(Transitions)
        {
        }
    }
}
