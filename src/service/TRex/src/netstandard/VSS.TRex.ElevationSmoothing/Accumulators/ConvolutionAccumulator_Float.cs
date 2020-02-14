namespace VSS.TRex.ElevationSmoothing
{
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
    }

    public override void Accumulate(float value, double coefficient)
    {
      if (value != NullValue)
      {
        sum += (float)(value * coefficient);
        NumNonNullValues++;
      }
    }

    public override void Clear()
    {
      NumNonNullValues = 0;
      sum = 0.0f;
    }

    public override float Result()
    {
      return NumNonNullValues > 0 ? sum  : NullValue;
    }
  }
}
