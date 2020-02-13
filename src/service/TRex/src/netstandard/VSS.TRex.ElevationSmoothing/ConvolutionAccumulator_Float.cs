namespace VSS.TRex.ElevationSmoothing
{
  public class ConvolutionAccumulator_Float : ConvolutionAccumulator<float>
  {
    protected float NullValue;
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
