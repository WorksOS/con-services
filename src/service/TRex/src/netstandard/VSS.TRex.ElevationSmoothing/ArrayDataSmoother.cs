using System;

namespace VSS.TRex.ElevationSmoothing
{
  /// <summary>
  /// Implements the capability to take a sub grid tree containing queried elevation data and apply algorithmic smoothing to the data
  /// </summary>
  public class ArrayDataSmoother<TV> : IArrayDataSmoother<TV>
  {
    private readonly TV[,] _source;
    private readonly IConvolutionTools<TV> _convolutionTools;
    private readonly int _contextSize;
    private readonly IConvolutionAccumulator<TV> _accumulator;
    private readonly Func<IConvolutionAccumulator<TV>, int, IConvolver<TV>> _convolverFactory;

    public ArrayDataSmoother(TV[,] source,
      IConvolutionTools<TV> convolutionTools, int contextSize,
      IConvolutionAccumulator<TV> accumulator,
      Func<IConvolutionAccumulator<TV>, int, IConvolver<TV>> convolverFactory)
    {
      if (_convolutionTools == null)
      {
        throw new ArgumentException("ConvolutionTools is null", nameof(convolutionTools));
      }

      _source = source;
      _convolutionTools = convolutionTools;
      _contextSize = contextSize;
      _accumulator = accumulator;
      _convolverFactory = convolverFactory;
    }

    public TV[,] Smooth()
    {
      var dest = new TV[_source.GetLength(0), _source.GetLength(1)];
      var convolver = _convolverFactory(_accumulator, _contextSize);

      _convolutionTools.Convolve(_source, dest, convolver);

      return dest;
    }
  }
}
