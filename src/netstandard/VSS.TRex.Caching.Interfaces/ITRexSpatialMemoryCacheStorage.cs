namespace VSS.TRex.Caching.Interfaces
{
  public interface ITRexSpatialMemoryCacheStorage<T>
  {
    int Add(T element, ITRexSpatialMemoryCacheContext context);
    void Remove(int index);
    T Get(int index);
    T Get(int index, out bool expired);
    int TokenCount { get; }
    bool HasFreeSpace();
    void EvictOneLRUItemWithLock();
  }
}
