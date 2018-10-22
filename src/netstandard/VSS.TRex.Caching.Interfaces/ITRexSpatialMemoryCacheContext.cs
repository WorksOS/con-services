using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Caching
{
  public interface ITRexSpatialMemoryCacheContext
  {
    IGenericSubGridTree_Long ContextTokens { get; }

    IMRURingBuffer<ITRexMemoryCacheItem> MRUList { get; }

    int TokenCount { get; }

    void Add(ITRexMemoryCacheItem element);

    void Remove(ISubGrid element);
  }
}
