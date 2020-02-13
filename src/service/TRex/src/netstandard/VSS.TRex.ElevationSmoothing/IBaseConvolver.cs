namespace VSS.TRex.ElevationSmoothing
{
  public interface IBaseConvolver<T>
  {
    int ContextSize { get; }

    void ConvolveElement(int i, int j);

    IConvolutionAccumulator<T> Accumulator { get; }
  }
}
