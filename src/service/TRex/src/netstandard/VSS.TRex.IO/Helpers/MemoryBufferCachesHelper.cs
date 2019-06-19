using VSS.TRex.DI;

namespace VSS.TRex.IO.Helpers
{
  public static class MemoryBufferCachesHelper
  {
    private static IMemoryBufferCaches _caches;
    public static IMemoryBufferCaches Caches => _caches ?? (_caches = DIContext.Obtain<IMemoryBufferCaches>());

    public static void Clear() => _caches = null;
  }
}
