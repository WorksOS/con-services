using System;

namespace VSS.TRex.DataSmoothing
{
  /// <summary>
  /// Implements the capability to take a sub grid tree containing queried elevation data and apply algorithmic smoothing to the data
  /// </summary>
  public class ArrayDataSmoother<TV> : IArrayDataSmoother<TV>
  {
    private readonly IConvolutionTools<TV> _convolutionTools;
    private readonly ConvolutionMaskSize _contextSize;
    private readonly IConvolutionAccumulator<TV> _accumulator;
    private readonly Func<IConvolutionAccumulator<TV>, ConvolutionMaskSize, IConvolver<TV>> _convolverFactory;

    public ArrayDataSmoother(
      IConvolutionTools<TV> convolutionTools, ConvolutionMaskSize contextSize,
      IConvolutionAccumulator<TV> accumulator,
      Func<IConvolutionAccumulator<TV>, ConvolutionMaskSize, IConvolver<TV>> convolverFactory)
    {
      _convolutionTools = convolutionTools ?? throw new ArgumentException("ConvolutionTools is null", nameof(convolutionTools));
      _contextSize = contextSize;
      _accumulator = accumulator;
      _convolverFactory = convolverFactory;
    }

    public int AdditionalBorderSize
    {
      get => (int)_contextSize / 2;
    }

    public TV[,] Smooth(TV[,] source)
    {
      var dest = new TV[source.GetLength(0), source.GetLength(1)];
      var convolver = _convolverFactory(_accumulator, _contextSize);

      _convolutionTools.Convolve(source, dest, convolver);

      return dest;
    }
  }
}
