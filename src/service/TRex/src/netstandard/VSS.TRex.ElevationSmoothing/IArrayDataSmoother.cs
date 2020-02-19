namespace VSS.TRex.DataSmoothing
{
  public interface IArrayDataSmoother<TV> : IDataSmoother
  {
    TV[,] Smooth(TV[,] source);
  }
}
