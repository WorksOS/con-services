namespace VSS.TRex.Caching.Interfaces
{
  public interface ITRexSpatialMemoryCacheStorage<T>
  {
    int Add(T element);
    int Remove(int index);
    int TokenCount { get; }
  }
}
