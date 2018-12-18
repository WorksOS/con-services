using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using VSS.TRex.Caching.Interfaces;

namespace VSS.TRex.Caching
{
  /// <summary>
  /// Implements a management thread that periodically checks the contexts in the cache for ones
  /// that are marked for removal and removes them. This is done in a single mutually exclusive lock
  /// within the main cache.
  /// </summary>
  public class TRexSpatialMemoryCacheContextRemover
  {
    private static readonly ILogger log = Logging.Logger.CreateLogger<TRexSpatialMemoryCacheContextRemover>();

    private readonly ITRexSpatialMemoryCache _cache;
    private readonly Thread _removalThread;
    private readonly int _sleepTimeMS;
    private readonly int _removalWaitTimeSeconds;

    private void PerformRemovalOperation()
    {
      while (_removalThread.ThreadState == ThreadState.Running)
      {
        try
        {
          // Instruct the cache to perform the cleanup...
          // Wait a time period minutes to remove items marked for removal
          _cache.RemoveContextsMarkedForRemoval(_removalWaitTimeSeconds);
        }
        catch (Exception e)
        {
          log.LogError("Exception thrown during RemoveContextsMarkedForRemoval()", e);
        }

        Thread.Sleep(_sleepTimeMS);
      }
    }

    public TRexSpatialMemoryCacheContextRemover(ITRexSpatialMemoryCache cache, int sleepTimeSeconds, int removalWaitTimeSeconds)
    {
      _cache = cache;
      _removalWaitTimeSeconds = removalWaitTimeSeconds;
      _sleepTimeMS = sleepTimeSeconds * 1000;
      _removalThread = new Thread(PerformRemovalOperation);
      _removalThread.Start();
    }
  }
}
