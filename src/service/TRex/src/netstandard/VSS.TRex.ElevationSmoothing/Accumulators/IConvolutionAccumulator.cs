namespace VSS.TRex.DataSmoothing
{
  public interface IConvolutionAccumulator<T>
  {
    T NullValue { get; set; }

    T ConvolutionSourceValue { get; set; }
    bool ConvolutionSourceValueIsNull();


    void Accumulate(T value);
    void Accumulate(T value, double coefficient);
    T Result();
    T NullInfillResult();
    void Clear();
  }
}
