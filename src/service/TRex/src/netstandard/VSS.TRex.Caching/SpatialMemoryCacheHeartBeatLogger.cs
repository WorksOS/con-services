using VSS.TRex.Caching.Interfaces;
using VSS.TRex.DI;

namespace VSS.TRex.Caching
{
  public class SpatialMemoryCacheHeartBeatLogger
  {
    public override string ToString()
    {
      ITRexSpatialMemoryCache cache = DIContext.Obtain<ITRexSpatialMemoryCache>();

      return $"General Result Cache: Context count = {cache?.ContextCount}, Project count = {cache?.ProjectCount}, Indicative size: {(1.0 * cache?.CurrentSizeInBytes)/1e6:F3}Mb";
    }
  }
}
