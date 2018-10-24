using VSS.TRex.Caching;

namespace VSS.TRex.Tests.Caching
{
  public class TRexSpatialMemoryCacheContextTests_Element : ITRexMemoryCacheItem
  {
    public int SizeInBytes { get; set; }

    public int IndicativeSizeInBytes() => SizeInBytes;

    public uint OriginX { get; set; }
    public uint OriginY { get; set; }
  }
}
