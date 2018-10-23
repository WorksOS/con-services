using VSS.TRex.Caching.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Caching
{
  public interface ITRexSpatialMemoryCacheContext
  {
    IGenericSubGridTree_Int ContextTokens { get; }

    //IMRURingBuffer<ITRexMemoryCacheItem> MRUList { get; }
    ITRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem> MRUList { get; }

    int TokenCount { get; }

    void Add(ITRexMemoryCacheItem element);

    void Remove(ITRexMemoryCacheItem element);

    ITRexMemoryCacheItem Get(uint originX, uint originY);
  }
}
