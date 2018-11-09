using VSS.TRex.Caching.Interfaces;
using VSS.TRex.DI;

namespace VSS.TRex.Caching
{
  /// <summary>
  /// Emits information relating to the current state and performance of the cache
  /// </summary>
  public class SpatialMemoryCacheHeartBeatLogger
  {
    public override string ToString()
    {
      ITRexSpatialMemoryCache cache = DIContext.Obtain<ITRexSpatialMemoryCache>();

      return $"General Result Cache: Item count = {cache.CurrentNumElements}/{cache.MaxNumElements} Context count = {cache?.ContextCount}, Project count = {cache?.ProjectCount}, Indicative size: {(1.0 * cache?.CurrentSizeInBytes)/1e6:F3}Mb/{(1.0 * cache?.MaxSizeInBytes) / 1e6:F3}Mb";
    }
  }
}
