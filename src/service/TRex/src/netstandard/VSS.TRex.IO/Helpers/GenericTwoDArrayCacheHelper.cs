using VSS.TRex.DI;

namespace VSS.TRex.IO.Helpers
{
    public static class GenericTwoDArrayCacheHelper<T>
    {
      public const int DEFAULT_TWOD_ARRAY_CACHE_SIZE = 15000;
      public const int DEFAULT_TWOD_DIMENSION_SIZE = 32;

      private static IGenericTwoDArrayCache<T> _caches;
      public static IGenericTwoDArrayCache<T> Caches()
      {
        if (_caches == null)
        {
          _caches = DIContext.Obtain<IGenericTwoDArrayCache<T>>() 
                    ?? new GenericTwoDArrayCache<T>(DEFAULT_TWOD_DIMENSION_SIZE, DEFAULT_TWOD_DIMENSION_SIZE, DEFAULT_TWOD_ARRAY_CACHE_SIZE);

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
