using Newtonsoft.Json;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class ColorValue
  {
    /// <summary>
    /// The color for the palette. 
    /// </summary>
    [JsonProperty(PropertyName = "color", Required = Required.Always)]
    //[Required]
    public uint Color { get; private set; }

    /// <summary>
    /// The discrete value or start of a range the color represents. 
    /// </summary>
    [JsonProperty(PropertyName = "value", Required = Required.Always)]
    //[Required]
    public double Value { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private ColorValue()
    { }

    /// <summary>
    /// Create instance of ColorValue
    /// </summary>
    public static ColorValue CreateColorValue(
      uint color,
      double value
    )
    {
      return new ColorValue
      {
        Color = color,
        Value = value
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {

    }
  }

}
