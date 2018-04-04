using System.Drawing;
using VSS.VisionLink.Raptor.Rendering.Palettes.Interfaces;

namespace VSS.VisionLink.Raptor.Rendering.Palettes
{
    // A basic palette class that defines a sdet of transitions covering a value range being rendered
    public class PaletteBase : IPlanViewPalette
    {
        public PaletteBase(Transition[] transitions)
        {
            PaletteTransitions = transitions;
        }

        // The set of transition value/colour pairs defining a renderable value range
        public Transition[] PaletteTransitions { get; set;}

        /// <summary>
        /// Logic to choose a colour from the set of transitions depending on the value. Slow but simple for the POC...
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Color ChooseColour(double value)
        {
            for (int i = PaletteTransitions.Length - 1; i >= 0; i--)
            {
                if (value >= PaletteTransitions[i].Value)
                {
                    return PaletteTransitions[i].Color;
                }
            }

            return Color.Empty;
        }
    }
}
