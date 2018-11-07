namespace ProductionDataSvc.AcceptanceTests.Models
{
    /// <summary>
    /// An association of a datum value expressed as a dimensionaless double value, and a colour, expressed as an RGB triplet encoded in an UInt32.
    /// This is a transition point - the location in a continuous series of colours comprising an overall set of colours to be used for rendering a thematic
    /// overlay tile. The series of colours is controlled by a set of transition points, each one being a ColorPalette.
    /// </summary>
    public class ColorPalette
    {
        /// <summary>
        /// The color related to the datum value
        /// </summary>
        public uint color { get; set; }

        /// <summary>
        /// The datum value at which the color defined in color should be used.
        /// </summary>
        public double value { get; set; }
    }

    public struct TColourPalette
    {
        public uint Colour;
        public double Value;
    }
}
