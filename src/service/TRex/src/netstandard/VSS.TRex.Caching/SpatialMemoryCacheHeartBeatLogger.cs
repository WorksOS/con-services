using Microsoft.Extensions.Logging;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.DI;

namespace VSS.TRex.Caching
{
  /// <summary>
  /// Emits information relating to the current state and performance of the cache
  /// </summary>
  public class SpatialMemoryCacheHeartBeatLogger: IHeartBeatLogger
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SpatialMemoryCacheHeartBeatLogger>();

    public void HeartBeat()
    {
      Log.LogInformation("Heartbeat: " + ToString());
    }

    public override string ToString()
    {
      var cache = DIContext.Obtain<ITRexSpatialMemoryCache>();

      return $"General Result Cache: Item count = {cache.CurrentNumElements}/{cache.MaxNumElements} Context count = {cache?.ContextCount}/{cache?.ContextRemovalCount}, Project count = {cache?.ProjectCount}, Removed Contexts = {cache.ContextRemovalCount}, Indicative size = {(1.0 * cache?.CurrentSizeInBytes)/1e6:F3}Mb/{(1.0 * cache?.MaxSizeInBytes) / 1e6:F3}Mb";
    }
  }
}
