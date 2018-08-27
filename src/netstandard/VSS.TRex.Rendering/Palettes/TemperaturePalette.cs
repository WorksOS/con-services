using Draw = System.Drawing;

namespace VSS.TRex.Rendering.Palettes
{
    /// <summary>
    /// Simple palette for rendering temperature data
    /// </summary>
    public class TemperaturePalette : PaletteBase
    {
        private static Transition[] Transitions =
        {
            new Transition(0, Draw.Color.Green),
            new Transition(200, Draw.Color.Yellow),
            new Transition(400, Draw.Color.Olive),
            new Transition(600, Draw.Color.Blue),
            new Transition(800, Draw.Color.SkyBlue),
            new Transition(1000, Draw.Color.Red)
        };

        public TemperaturePalette() : base(Transitions)
        {
        }
    }
}
