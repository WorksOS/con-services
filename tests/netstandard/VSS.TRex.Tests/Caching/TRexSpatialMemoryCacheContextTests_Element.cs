using VSS.TRex.Caching.Interfaces;

namespace VSS.TRex.Tests.Caching
{
  public class TRexSpatialMemoryCacheContextTests_Element : ITRexMemoryCacheItem
  {
    public int SizeInBytes { get; set; }

    public int IndicativeSizeInBytes() => SizeInBytes;

    public uint CacheOriginX { get; set; }
    public uint CacheOriginY { get; set; }

    public override string ToString() => $"{CacheOriginX}:{CacheOriginY}:{SizeInBytes}";
  }
}
