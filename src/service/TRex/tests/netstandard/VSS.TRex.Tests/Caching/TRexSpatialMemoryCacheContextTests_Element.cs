using VSS.TRex.Caching.Interfaces;

namespace VSS.TRex.Tests.Caching
{
  public class TRexSpatialMemoryCacheContextTests_Element : ITRexMemoryCacheItem
  {
    public int SizeInBytes { get; set; }

    public int IndicativeSizeInBytes() => SizeInBytes;

    public int CacheOriginX { get; set; }
    public int CacheOriginY { get; set; }

    public override string ToString() => $"{CacheOriginX}:{CacheOriginY}:{SizeInBytes}";
  }
}
