namespace VSS.TRex.DataSmoothing
{
  public class WeightedMeanFilter<T> : MeanFilter<T>
  {
    public WeightedMeanFilter(IConvolutionAccumulator<T> accumulator,
      ConvolutionMaskSize contextSize, double centerWeight, NullInfillMode nullInfillMode) 
      : base(accumulator, CreateFilter(contextSize, centerWeight), nullInfillMode)
    {
    }
  }
}
