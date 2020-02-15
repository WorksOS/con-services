namespace VSS.TRex.ElevationSmoothing
{
  public interface IArrayDataSmoother<TV> : IDataSmoother
  {
    TV[,] Smooth();
  }
}
