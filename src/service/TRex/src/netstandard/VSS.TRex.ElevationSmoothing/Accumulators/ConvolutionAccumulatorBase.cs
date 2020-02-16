namespace VSS.TRex.ElevationSmoothing
{
  public abstract class ConvolutionAccumulator<T> : IConvolutionAccumulator<T>
  {
    public T NullValue { get; set; }
    public T ConvolutionSourceValue { get; set; }
    public abstract void Accumulate(T value);
    public abstract void Accumulate(T value, double coefficient);
    public abstract T Result();
    public abstract void Clear();

    public abstract bool ConvolutionSourceValueIsNull();
  }
}
