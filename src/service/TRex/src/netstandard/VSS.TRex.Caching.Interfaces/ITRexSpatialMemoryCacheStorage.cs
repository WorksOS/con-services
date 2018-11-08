namespace VSS.TRex.Caching.Interfaces
{
  public interface ITRexSpatialMemoryCacheStorage<T>
  {
    int Add(T element, ITRexSpatialMemoryCacheContext context);
    void Remove(int index);
    T Get(int index);
    int TokenCount { get; }
    bool HasFreeSpace();
    void EvictOneLRUItemWithLock();
    void Invalidate(int index);
  }
}
