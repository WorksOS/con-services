namespace VSS.TRex.Caching
{
  public interface ITRexSpatialMemoryCache
  {
    int MaxNumElements { get; }

    int CurrentNumElements { get; }

    void ItemAddedToContext(int sizeInBytes);
    void ItemRemovedFromContext(int sizeInBytes);
  }
}
