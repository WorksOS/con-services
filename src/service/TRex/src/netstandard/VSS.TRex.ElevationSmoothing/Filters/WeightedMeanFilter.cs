namespace VSS.TRex.ElevationSmoothing
{
  public class WeightedMeanFilter<T> : MeanFilter<T>
  {
    public WeightedMeanFilter(IConvolutionAccumulator<T> accumulator, int contextSize, double centerWeight) : base(accumulator, CreateFilter(contextSize, centerWeight))
    {
    }
  }
}
