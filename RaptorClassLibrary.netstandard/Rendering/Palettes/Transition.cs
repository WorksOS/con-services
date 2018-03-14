using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Rendering.Palettes
{
    /// <summary>
    /// Transition represents a point on a value line being visualised where a new color starts being used to render the interval of values above the transition value
    /// </summary>
    public struct Transition
    {
        public double Value; 
        public Color Color;

        public Transition(double value, Color color)
        {
            Value = value;
            Color = color;
        }
    }
}
