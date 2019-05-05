using System;

namespace VSS.TRex.Rendering.Palettes.CCAColorScale
{
  /// <summary>
  ///  Color scales are made up of a collection of CCAolorScaleSegments.
  ///
  ///  Segments have min and max values which are used by the color scale for
  ///  looking up values. For historical reasons the lookup is not 100% intuitive.
  ///  The look up check looks like this if the min/max values are different:
  ///
  ///  value less than the max and greater or equal to the min
  ///
  ///  Note that the check includes the min value but excludes the max value. This
  ///  means that if you want to segments to cover a continuous range you need to
  ///  set the min of the following segment to the max of the preceding segment.
  ///  As an example, lets say we have three segments for a scale which runs from
  ///  1 to 30 inclusive. This is how you would set the min/max values
  ///
  ///  Segment 1: min = 1, max = 10
  ///  Segment 2: min = 10, max = 20
  ///  Segment 3: min = 20, max = 31
  ///
  ///  Note that the max value for the last segment is 31 rather than 30. This is
  ///  because the max value is not included in the lookup.
  ///
  /// If you want a segment to represent a single value set the min and max to the
  ///  same value.
  /// </summary>
  public class CCAColorScaleSegment
  {
    /// <summary>
    /// The minimum value for the segment.
    /// </summary>
    public int MinValue { get; set; }
    /// <summary>
    /// The maximum value for the segment.
    /// </summary>
    public int MaxValue { get; set; }
    /// <summary>
    /// The base color of the segment. Gradients are interpolated using this colour as the starting point.
    /// </summary>
    public uint Color { get; set; }
    /// <summary>
    /// Set to true if the segment should use a hatched brush for drawing.
    /// </summary>
    public bool IsHatched { get; set; }

    /// <summary>
    /// Public default constructor.
    /// </summary>
    public CCAColorScaleSegment()
    {
    }

    /// <summary>
    /// Override constructor with parameters.
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <param name="color"></param>
    /// <param name="isHatched"></param>
    public CCAColorScaleSegment(int value1, int value2, uint color, bool isHatched)
    {
      SetValueRange(value1, value2);

      Color = color;
      IsHatched = isHatched;
    }

    /// <summary>
    ///  Checks if a value is within the min/max values for the segment
    ///
    ///  This function checks if a value is within the min/max values for the segment.
    ///  The check looks like this if the min and max are different:
    ///  
    ///  value less than the max and greater or equal to the min
    /// 
    ///  Note that the check includes the min value but excludes the max value. See the
    ///  class description for more information on setting this values.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool IsValueWithinRange(int value)
    {
      if (MinValue == MaxValue)
      {
        if (value == MaxValue)
        return true;
      }
      else if (value >= MinValue && value < MaxValue)
        return true;

      return false;
    }

    /// <summary>
    ///  Set the range of values for the segment
    ///
    ///  This function sets the range of values for the segment. This is the preferred way
    ///  to set the value range as it handles assigning the values to the min and max values,
    ///  thereby avoiding any problems. For a segment representing a single value you can
    ///  pass in the same value for each parameter.
    ///
    ///  See the class description for more information on how the min and max values for
    ///  a segment should be set up.
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    public void SetValueRange(int value1, int value2)
    {
      MinValue = Math.Min(value1, value2);
      MaxValue = Math.Max(value1, value2);
    }
  }
}
