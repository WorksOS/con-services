using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Rendering.Palettes
{
    /// <summary>
    /// Simple palette for rendering temperature data
    /// </summary>
    public class TemperaturePalette : PaletteBase
    {
        private static Transition[] Transitions =
        {
            new Transition(0, Color.Green),
            new Transition(200, Color.Yellow),
            new Transition(400, Color.Olive),
            new Transition(600, Color.Blue),
            new Transition(800, Color.SkyBlue),
            new Transition(1000, Color.Red)
        };

        public TemperaturePalette() : base(Transitions)
        {
        }
    }
}
