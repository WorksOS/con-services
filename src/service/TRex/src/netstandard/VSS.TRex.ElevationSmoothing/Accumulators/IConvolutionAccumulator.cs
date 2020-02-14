namespace VSS.TRex.ElevationSmoothing
{
  public interface IConvolutionAccumulator<T>
  {
    T NullValue { get; set; }
    void Accumulate(T value);
    void Accumulate(T value, double coefficient);
    T Result();
    void Clear();
  }
}
