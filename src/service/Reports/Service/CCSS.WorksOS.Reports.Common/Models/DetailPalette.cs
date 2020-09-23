using System.Collections.Generic;
using Newtonsoft.Json;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class DetailPalette
  {
    /// <summary>
    /// The color/value pairs for the palette. There must be at least one item in the list.
    /// The values must be in ascending order. For integral values (e.g. pass count) the color is
    /// used for the exact value. For continuous values (e.g. elevation) the color is used for all
    /// values that fall with the range from the value upto but excluding the next value in the list.
    /// </summary>
    [JsonProperty(PropertyName = "colorValues", Required = Required.Always)]
    //[Required]
    public List<ColorValue> ColorValues { get; private set; }

    /// <summary>
    /// The color for values above the last value. 
    /// </summary>
    [JsonProperty(PropertyName = "aboveLastColor", Required = Required.Default)]
    public uint? AboveLastColor { get; private set; }

    /// <summary>
    /// The color for values below the first value. 
    /// </summary>
    [JsonProperty(PropertyName = "belowFirstColor", Required = Required.Default)]
    public uint? BelowFirstColor { get; private set; }
  }
}
