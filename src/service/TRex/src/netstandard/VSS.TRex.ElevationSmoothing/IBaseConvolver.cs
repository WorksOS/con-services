namespace VSS.TRex.ElevationSmoothing
{
  public interface IBaseConvolver
  {
    int ContextSize { get; }

    void ConvolveElement(int i, int j);
  }
}
