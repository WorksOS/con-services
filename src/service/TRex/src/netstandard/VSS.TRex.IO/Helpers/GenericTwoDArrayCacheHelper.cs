using VSS.TRex.DI;

namespace VSS.TRex.IO.Helpers
{
    public static class GenericTwoDArrayCacheHelper<T>
    {
      private static IGenericTwoDArrayCache<T> _caches;
      public static IGenericTwoDArrayCache<T> Caches()
      {
        if (_caches == null)
        {
          _caches = DIContext.Obtain<IGenericTwoDArrayCache<T>>() ?? new GenericTwoDArrayCache<T>(32, 32, 1000);
          GenericTwoDArrayCacheRegister.Add(_caches);
        }

        return _caches;
      }

      public static void Clear()
      {
        _caches = null;
      }
    }
}
