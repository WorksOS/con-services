using System;

namespace VSS.TRex.DataSmoothing
{
  /// <summary>
  /// Implements the capability to take a sub grid tree containing queried elevation data and apply algorithmic smoothing to the data
  /// </summary>
  public class ArrayDataSmoother<TV> : IArrayDataSmoother<TV>
  {
    private readonly IConvolutionTools<TV> _convolutionTools;
    private readonly int _contextSize;
    private readonly IConvolutionAccumulator<TV> _accumulator;
    private readonly Func<IConvolutionAccumulator<TV>, int, IConvolver<TV>> _convolverFactory;

    public ArrayDataSmoother(
      IConvolutionTools<TV> convolutionTools, int contextSize,
      IConvolutionAccumulator<TV> accumulator,
      Func<IConvolutionAccumulator<TV>, int, IConvolver<TV>> convolverFactory)
    {
      _convolutionTools = convolutionTools ?? throw new ArgumentException("ConvolutionTools is null", nameof(convolutionTools));
      _contextSize = contextSize;
      _accumulator = accumulator;
      _convolverFactory = convolverFactory;
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
