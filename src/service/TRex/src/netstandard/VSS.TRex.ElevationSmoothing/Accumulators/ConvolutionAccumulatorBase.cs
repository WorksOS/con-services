namespace VSS.TRex.DataSmoothing
{
  public abstract class ConvolutionAccumulator<T> : IConvolutionAccumulator<T>
  {
    public T NullValue { get; set; }

    protected T _convolutionSourceValue;
    public T ConvolutionSourceValue
    {
      get => _convolutionSourceValue;
      set
      {
        _convolutionSourceValue = value;
        _convolutionSourceValueIsNull = ConvolutionSourceValueIsNull();
      }
    }

    protected bool _convolutionSourceValueIsNull;

    public abstract void Accumulate(T value);
    public abstract void Accumulate(T value, double coefficient);
    public abstract T Result();
    public abstract T NullInfillResult();
    public abstract void Clear();

    public abstract bool ConvolutionSourceValueIsNull();
  }
}
