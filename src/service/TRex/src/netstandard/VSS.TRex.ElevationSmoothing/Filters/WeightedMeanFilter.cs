namespace VSS.TRex.DataSmoothing
{
  public class WeightedMeanFilter<T> : MeanFilter<T>
  {
    public WeightedMeanFilter(IConvolutionAccumulator<T> accumulator, 
      int contextSize, double centerWeight, 
      bool updateNullValues, bool infillNullValuesOnly) 
      : base(accumulator, CreateFilter(contextSize, centerWeight), updateNullValues, infillNullValuesOnly)
    {
    }
  }
}
