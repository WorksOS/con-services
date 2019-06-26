using VSS.TRex.DI;

namespace VSS.TRex.IO.Helpers
{
    public static class GenericTwoDArrayCacheHelper<T>
    {
      private static ITwoDArrayCache<T> _caches;
      public static ITwoDArrayCache<T> Caches => _caches ?? (_caches = DIContext.Obtain<ITwoDArrayCache<T>>());

      public static void Clear() => _caches = null;
    }
}
