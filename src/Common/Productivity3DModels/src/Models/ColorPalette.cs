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
    public uint Color { get; set; }

    /// <summary>
    /// The datum value at which the color defined in color should be used.
    /// </summary>
    [JsonProperty(PropertyName = "value", Required = Required.Always)]
    [Required]
    public double Value { get; private set; }

    /// <summary>
    /// Default private constructor
    /// </summary>
    private ColorPalette()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="value"></param>
    public ColorPalette(uint color, double value)
    {
      Color = color;
      Value = value;
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
