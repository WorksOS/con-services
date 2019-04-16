using System.Collections.Generic;

namespace VSS.TRex.Rendering.Palettes.CCAColorScale
{
  /// <summary>
  /// The CCAColorScale class is used to provide color look up for drawing both mapping and legends for mapping of CCA data.
  /// Color scales are made up of segments, represented by the CCAColorScaleSegment class.
  /// </summary>
  public class CCAColorScale
  {
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

      //if (rangeMin != rangeMax)
      //{
      //  if (!InvertGradient)
      //    blackMix = 50 * (value - rangeMin) div (rangeMax - rangeMin);
      //  else
      //BlackMix:= (50 * (RangeMax - Value)) div(RangeMax - RangeMin);
      //}

      //RGB:= ColorToRGB(BaseColor);

      //Result:= RGBToColor(RGB.R - (BlackMix * RGB.R) div 100,
      //                    RGB.G - (BlackMix * RGB.G) div 100,
      //                    RGB.B - (BlackMix * RGB.B) div 100);
    }

  }
}
