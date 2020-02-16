namespace VSS.TRex.ElevationSmoothing
{
  /// <summary>
  /// Provides a type specific accumulator behaviour for floats (eg: elevation values).
  /// This used a convolutionfilter with coefficients that sum to 1. Each value is multiplied by the supplied
  /// coefficient. If the value being considered is null then the value of te cell being convolved (at the
  /// senter of the convolutiokn filter) is multiplied by the supplied filter coeffient and added to sum.
  /// </summary>
  public class ConvolutionAccumulator_Float : ConvolutionAccumulator<float>
  {
    public int NumNonNullValues;
    protected float sum;

    public ConvolutionAccumulator_Float(float nullValue)
    {
      NullValue = nullValue;
    }

    public override void Accumulate(float value)
    {
      if (value != NullValue)
      {
        sum += value;
        NumNonNullValues++;
      }
      else
      {
        sum += ConvolutionSourceValue;
      }
    }

    public override void Accumulate(float value, double coefficient)
    {
      if (value != NullValue)
      {
        sum += (float)(value * coefficient);
        NumNonNullValues++;
      }
      else
      {
        sum += (float)(ConvolutionSourceValue * coefficient);
      }
    }

    public override void Clear()
    {
      NumNonNullValues = 0;
      sum = 0.0f;
      ConvolutionSourceValue = 0.0f;
    }

    public override bool ConvolutionSourceValueIsNull() => ConvolutionSourceValue == NullValue;

    public override float Result()
    {
      return NumNonNullValues > 0 ? sum  : NullValue;
    }
  }
}
