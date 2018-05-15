using System.Drawing;

namespace VSS.TRex.Rendering.Palettes
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
