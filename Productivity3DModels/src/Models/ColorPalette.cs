using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace VSS.Productivity3D.Models.Models
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
    [JsonProperty(PropertyName = "color", Required = Required.Always)]
    [Required]
    public uint color { get; private set; }

    /// <summary>
    /// The datum value at which the color defined in color should be used.
    /// </summary>
    [JsonProperty(PropertyName = "value", Required = Required.Always)]
    [Required]
    public double value { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private ColorPalette()
    { }

    /// <summary>
    /// Create instance of ColorPalette
    /// </summary>
    public static ColorPalette CreateColorPalette(uint color, double value)
    {
      return new ColorPalette
      {
        color = color,
        value = value
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      //Nothing else to validate
    }
  }
}