using System.Collections.Generic;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionColorPalettesResult : ResponseBase
  {
    /// <summary>
    /// The palette for displaying CMV detail values.
    /// </summary>
    public DetailPalette cmvDetailPalette { get; set; }
    /// <summary>
    /// The palette for displaying pass count detail values.
    /// </summary>
    public DetailPalette passCountDetailPalette { get; set; }
    /// <summary>
    /// The palette for displaying pass count summary values.
    /// </summary>
    public SummaryPalette passCountSummaryPalette { get; set; }
    /// <summary>
    /// The palette for displaying cut/fill values.
    /// </summary>
    public DetailPalette cutFillPalette { get; set; }
    /// <summary>
    /// The palette for displaying temperature summary values.
    /// </summary>
    public SummaryPalette temperatureSummaryPalette { get; set; }
    /// <summary>
    /// The palette for displaying CMV summary values.
    /// </summary>
    public SummaryPalette cmvSummaryPalette { get; set; }
    /// <summary>
    /// The palette for displaying MDP summary values.
    /// </summary>
    public SummaryPalette mdpSummaryPalette { get; set; }
    /// <summary>
    /// The palette for displaying CMV % change values.
    /// </summary>
    public DetailPalette cmvPercentChangePalette { get; set; }
    /// <summary>
    /// The palette for displaying speed summary values.
    /// </summary>
    public SummaryPalette speedSummaryPalette { get; set; }
    /// <summary>
    /// The palette for displaying temperature details values.
    /// </summary>
    public DetailPalette temperatureDetailPalette { get; set; }

    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionColorPalettesResult()
            : base("success")
    { }
  }

  /// <summary>
  /// Representation of a palette for details data, both integral (e.g. pass count) and continuous (e.g elevation)
  /// </summary>
  public class DetailPalette 
  {
    /// <summary>
    /// The color/value pairs for the palette. There must be at least one item in the list.
    /// The values must be in ascending order. For integral values (e.g. pass count) the color is
    /// used for the exact value. For continuous values (e.g. elevation) the color is used for all
    /// values that fall with the range from the value upto but excluding the next value in the list.
    /// </summary>
    public List<ColorValue> colorValues { get; set; }

    /// <summary>
    /// The color for values above the last value. 
    /// </summary>
    public uint? aboveLastColor { get; set; }

    /// <summary>
    /// The color for values below the first value. 
    /// </summary>
    public uint? belowFirstColor { get; set; }
  }

  /// <summary>
  /// Representation of a palette for summary data
  /// </summary>
  public class SummaryPalette 
  {
    /// <summary>
    /// The color for values above the target. 
    /// </summary>
    public uint aboveTargetColor { get; set; }

    /// <summary>
    /// The color for values equal to a target or within a target range. 
    /// </summary>
    public uint onTargetColor { get; set; }

    /// <summary>
    /// The color for values below the target. 
    /// </summary>
    public uint belowTargetColor { get; set; }
  }

  /// <summary>
  /// Represents a color and value pair for a palette
  /// </summary>
  public class ColorValue 
  {
    /// <summary>
    /// The color for the palette. 
    /// </summary>
    public uint color { get; set; }

    /// <summary>
    /// The discrete value or start of a range the color represents. 
    /// </summary>
    public double value { get; set; }
  }
}
