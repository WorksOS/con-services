namespace VSS.TRex.ElevationSmoothing
{
  public class ConvolutionAccumulator_Float : ConvolutionAccumulator<float>
  {
    protected int numValues;
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
        numValues++;
      }
    }

    public override void Accumulate(float value, double coefficient)
    {
      if (value != NullValue)
      {
        sum += (float)(value * coefficient);
        numValues++;
      }
    }

    public override void Clear()
    {
      numValues = 0;
      sum = 0.0f;
    }

    public override float Result()
    {
      return numValues > 0 ? (sum / numValues) : NullValue;
    }
  }
}
