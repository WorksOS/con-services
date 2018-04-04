using System.Drawing;

namespace VSS.VisionLink.Raptor.Rendering.Palettes.Interfaces
{
    /// <summary>
    /// IPlanViewPalette defines the responsibilty of deriving a colour value from a datum supplied to it
    /// </summary>
    public interface IPlanViewPalette
    {
        /// <summary>
        /// Returns a Color derived from the given value datum
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Color ChooseColour(double value);
    }
}
