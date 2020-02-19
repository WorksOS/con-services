namespace VSS.TRex.DataSmoothing
{
  /// <summary>
  /// Provides a type specific accumulator behaviour for floats (eg: elevation values).
  /// This used a convolutionfilter with coefficients that sum to 1. Each value is multiplied by the supplied
  /// coefficient. If the value being considered is null then the value of te cell being convolved (at the
  /// senter of the convolutiokn filter) is multiplied by the supplied filter coeffient and added to sum.
  /// </summary>
  public class ConvolutionAccumulator_Float : ConvolutionAccumulator<float>
  {
    public int NumNonNullValues;
    private float _sum;
    private bool _sumIsNull;
    private readonly ConvolutionMaskSize _contextSize;
    private readonly float _contextSizeSquare;

    public ConvolutionAccumulator_Float(float nullValue, ConvolutionMaskSize contextSize)
    {
      NullValue = nullValue;
      _contextSize = contextSize;
      _contextSizeSquare = (int)_contextSize * (int)_contextSize;
      Clear();
    }

    public override void Accumulate(float value) => Accumulate(value, 1.0);

    public override void Accumulate(float value, double coefficient)
    {
      if (value != NullValue)
      {
        _sum = _sumIsNull ? (float)(value * coefficient) : _sum + (float)(value * coefficient);
        NumNonNullValues++;
      }
      else
      {
        if (!_convolutionSourceValueIsNull)
        {
          _sum = _sumIsNull ? (float) (_convolutionSourceValue * coefficient) : _sum + (float) (_convolutionSourceValue * coefficient);
        }
      }

      _sumIsNull = false;
    }

    public override void Clear()
    {
      NumNonNullValues = 0;
      _sum = NullValue;
      _sumIsNull = true;
      _convolutionSourceValue = NullValue;
      _convolutionSourceValueIsNull = true;
    }

    public override bool ConvolutionSourceValueIsNull() => _convolutionSourceValue == NullValue;

    public override float Result() => _sum;

    public override float NullInfillResult()
    {
      const float minimumConsensusFraction = 0.5f;
      var concensusFraction = NumNonNullValues / _contextSizeSquare;

      if (concensusFraction > minimumConsensusFraction)
      {
        return (_contextSizeSquare / NumNonNullValues) * _sum;
      }

      return NullValue;
    }
  }
}
