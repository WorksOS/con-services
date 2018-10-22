namespace VSS.TRex.Caching
{
  public interface ITRexSpatialMemoryCache
  {
    int MaxNumElements { get; }

    int CurrentNumElements { get; }
  }
}
