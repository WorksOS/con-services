using System.Collections.Generic;
using System.Drawing;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.Rendering.Palettes.CCAColorScale
{
  /// <summary>
  /// // Creates a CCA data coverage scale....
  /// </summary>
  public static class CCAColorScaleManager
  {
    /// <summary>
    /// //  Creates the old style coverage scale used to generate a new style coverage scale...
    /// </summary>
    /// <param name="ccaRequiredMinimumPasses"></param>
    /// <param name="ccaColorScale"></param>
    public static void CreateInitialCoverageScale(short ccaRequiredMinimumPasses, ref CCAColorScale ccaColorScale)
    {
      const short CCV_COLOR_SCALE_COUNT = 4;

      var requiredColors = (ccaRequiredMinimumPasses * 2) - 1;

      var colorsArray = new Color [CCV_COLOR_SCALE_COUNT + 1];

      colorsArray[0] = Color.DarkGray;
      colorsArray[1] = Color.Green;
      colorsArray[2] = Color.Aqua;
      colorsArray[3] = Color.Red;
      colorsArray[4] = Color.Yellow;

      if (requiredColors > 1)
        requiredColors--;

      ccaColorScale.IsSolidColors = false;
      ccaColorScale.InvertGradient = false;

      // How many color points are we going to use?..
      var numberOfColorPoints = requiredColors < CCV_COLOR_SCALE_COUNT + 1 ? requiredColors : CCV_COLOR_SCALE_COUNT;

      var colorSegments = ccaColorScale.ColorSegments;

      for (var i = 1; i <= numberOfColorPoints; i++)
        colorSegments.Add(new CCAColorScaleSegment { Color =  ColorUtility.ColorToUInt(colorsArray[i].R, colorsArray[i].G, colorsArray[i].B) });

      // Max value of this is always the required passes...
      colorSegments[0].SetValueRange(ccaRequiredMinimumPasses, ccaRequiredMinimumPasses);

      float passesLeft = ccaRequiredMinimumPasses - 1;
      var step = passesLeft / (colorSegments.Count - 1);

      // Scale rest of colors evenly over passes left...
      for (var i = 1; i <= colorSegments.Count - 1; i++)
      {
        var value1 = (short)(passesLeft + 1);
        passesLeft -= step;
        var value2 = (short)passesLeft;

        if (value2 != 0 || i == colorSegments.Count - 2)
          value2++;

        colorSegments[i].SetValueRange(value1, value2);
      }
    }

    public static CCAColorScale CreateCoverageScale(short ccaRequiredMinimumPasses)
    {
      var ccaColorScale = new CCAColorScale(new List<CCAColorScaleSegment>());

      // From the CTCT Tonka source code...
      //
      // This might seem a little odd, but what we do here is use
      // CreateInitialCoverageScale() to create a colour scale emulating the way
      // the old code used to do it. The old colour scale was quite complex and
      // although it looked like separate blocks of colour it was actually using
      // a few solid colour blocks with interpolated colours. It was very tricky
      // to get things to match when doing lookups and such though. So what we do
      // is recreate the old scale and then query it to build a new scale of
      // individual blocks of colour.
      CreateInitialCoverageScale(ccaRequiredMinimumPasses, ref ccaColorScale);

      var colorSegments = new List<CCAColorScaleSegment>();

      var isHatched = false;

      for (var i = ccaRequiredMinimumPasses; i >= 1; i--)
      {
        var tempColor = ccaColorScale.Lookup(i, ref isHatched);
        var newColorSegment = new CCAColorScaleSegment(i, i, tempColor, isHatched);
        colorSegments.Insert(colorSegments.Count, newColorSegment);
      }

      ccaColorScale.IsSolidColors = true;
      ccaColorScale.HasTarget = true;
      ccaColorScale.ColorSegments = colorSegments;

      return ccaColorScale;
    }
  }
}
