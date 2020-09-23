using Newtonsoft.Json;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class SummaryPalette
  {
    /// <summary>
    /// The color for values above the target. 
    /// </summary>
    [JsonProperty(PropertyName = "aboveTargetColor", Required = Required.Always)]
    //[Required]
    public uint AboveTargetColor { get; private set; }

    /// <summary>
    /// The color for values equal to a target or within a target range. 
    /// </summary>
    [JsonProperty(PropertyName = "onTargetColor", Required = Required.Always)]
    //[Required]
    public uint OnTargetColor { get; private set; }

    /// <summary>
    /// The color for values below the target. 
    /// </summary>
    [JsonProperty(PropertyName = "belowTargetColor", Required = Required.Always)]
    //[Required]
    public uint BelowTargetColor { get; private set; }


    /// <summary>
    /// Private constructor
    /// </summary>
    private SummaryPalette()
    { }

    /// <summary>
    /// Create instance of SummaryPalette
    /// </summary>
    public static SummaryPalette CreateSummaryPalette(
      uint aboveTargetColor,
      uint onTargetColor,
      uint belowTargetColor
    )
    {
      return new SummaryPalette
      {
        AboveTargetColor = aboveTargetColor,
        OnTargetColor = onTargetColor,
        BelowTargetColor = belowTargetColor
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
