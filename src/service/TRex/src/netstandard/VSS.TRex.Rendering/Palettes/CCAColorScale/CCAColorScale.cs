using System.Collections.Generic;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.Rendering.Palettes.CCAColorScale
{
  /// <summary>
  /// The CCAColorScale class is used to provide color look up for drawing both mapping and legends for mapping of CCA data.
  /// Color scales are made up of segments, represented by the CCAColorScaleSegment class.
  /// </summary>
  public class CCAColorScale
  {
    private const int COLOR_RED_VALUE = 255;
    private const int COLOR_GREEN_VALUE = 255;
    private const int COLOR_BLUE_VALUE = 255;

    /// <summary>
    /// The segments which make up the scale.
    /// </summary>
    public List<CCAColorScaleSegment> ColorSegments { get; set; }
    /// <summary>
    /// Set to true if the scale should show target bars when a legend is drawn.
    /// </summary>
    public bool HasTarget { get; set; }
    /// <summary>
    /// Set to true if the scale only uses solid colours, as opposed to having a gradient within each segment.
    /// Hatched colours count as solid.
    /// </summary>
    public bool IsSolidColors { get; set; }
    /// <summary>
    /// Set to true to invert the gradient used for the scale segments.
    /// </summary>
    public bool InvertGradient { get; set; }
    /// <summary>
    /// The number of shades used for drawing each segment of a legend when using a gradient.
    /// </summary>
    public short NumLegendShades { get; set; }

    /// <summary>
    /// Public default constructor.
    /// </summary>
    public CCAColorScale()
    {
    }

    /// <summary>
    /// Override constructor with parameters.
    /// </summary>
    /// <param name="colorSegments"></param>
    /// <param name="isSolidColors"></param>
    /// <param name="hasTarget"></param>
    /// <param name="invertGradient"></param>
    /// <param name="numSegmentShadesForLegend"></param>
    public CCAColorScale(
      List<CCAColorScaleSegment> colorSegments,
      bool isSolidColors = false,
      bool hasTarget = false,
      bool invertGradient = false,
      short numSegmentShadesForLegend = 0)
    {
      ColorSegments = colorSegments;
      IsSolidColors = isSolidColors;
      HasTarget = hasTarget;
      InvertGradient = invertGradient;
      NumLegendShades = numSegmentShadesForLegend;
    }

    /// <summary>
    /// This function interpolates shades of the color scale.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="rangeMin"></param>
    /// <param name="rangeMax"></param>
    /// <param name="baseColor"></param>
    /// <returns></returns>
    private uint ScaleColor(int value, int rangeMin, int rangeMax, uint baseColor)
    {
      // Mix with 50% black at max, and 0% black at min...
      var blackMix = 0;

      if (rangeMin != rangeMax)
      {
        var colorValue = !InvertGradient ? value - rangeMin : rangeMax - value;

        blackMix = 50 * colorValue / (rangeMax - rangeMin);
      }

      var color = ColorUtility.UIntToColor(baseColor);

      var r = color.R - (blackMix * color.R) / 100;
      var g = color.G - (blackMix * color.G) / 100;
      var b = color.B - (blackMix * color.B) / 100;

      return ColorUtility.ColorToUInt(r, g, b);
    }

    /// <summary>
    /// Looks up for the colour in the list of the colour segments.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isHatched"></param>
    /// <returns></returns>
    public uint Lookup(int value, ref bool isHatched)
    {
      if (ColorSegments.Count == 0)
        return ColorUtility.ColorToUInt(COLOR_RED_VALUE, COLOR_GREEN_VALUE, COLOR_BLUE_VALUE);

      int minValue;
      int maxValue;

      for (var i = ColorSegments.Count - 1; i >= 0; i--)
      {
        isHatched = ColorSegments[i].IsHatched;

        if (!IsSolidColors)
        {
          minValue = ColorSegments[i].MinValue;
          maxValue = ColorSegments[i].MaxValue;

          return  ScaleColor(value, minValue, maxValue, ColorSegments[i].Color);
        }

        return ColorSegments[i].Color;
      }

      // Off the top, scaled to max...
      var tempColor = ColorSegments[0].Color;

      minValue = ColorSegments[0].MinValue;
      maxValue = ColorSegments[0].MaxValue;

      isHatched = ColorSegments[0].IsHatched;

      if (minValue != maxValue)
        tempColor = ScaleColor(maxValue, minValue, maxValue, ColorSegments[0].Color);

      return tempColor;
    }

    /// <summary>
    ///  Returns the number of colors in the scale.
    ///  This function returns the number of colors in the scale. It effectively
    ///  corresponds to the number of segments in the scale. It returns the same
    ///  value whether a scale uses solid colors or a gradient.
    /// </summary>
    public int TotalColors => ColorSegments.Count;

    public uint GetColorAtIndex(int index, ref bool isHatched)
    {
      var tempColor = ColorUtility.ColorToUInt(COLOR_RED_VALUE, COLOR_GREEN_VALUE, COLOR_BLUE_VALUE);

      if (index >= 0 && index < ColorSegments.Count)
      {
        tempColor = ColorSegments[index].Color;
        isHatched = ColorSegments[index].IsHatched;
      }

      return tempColor;
    }

    /// <summary>
    /// Gets the min and max values for the segment at the specified index.
    /// </summary>
    /// <param name="segmentIndex"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public void GetSegmentMinMax(int segmentIndex, out int min, out int max)
    {
      min = 0;
      max = int.MaxValue;

      if (segmentIndex >= 0 && segmentIndex < ColorSegments.Count)
      {
        min = ColorSegments[segmentIndex].MinValue;
        max = ColorSegments[segmentIndex].MaxValue;
      }
    }

  }
}
