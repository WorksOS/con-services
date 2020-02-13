namespace VSS.TRex.ElevationSmoothing
{
  public interface IConvolutionAccumulator<T>
  {
    T NullValue { get; set; }

    void Accumulate(T value);
    T Result();
    void Clear();


  }

  public abstract class ConvolutionAccumulator<T> : IConvolutionAccumulator<T>
  {
    public T NullValue { get; set; }

    public abstract void Accumulate(T value);
    public abstract T Result();
    public abstract void Clear();
  }
}
