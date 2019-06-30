using VSS.TRex.DI;

namespace VSS.TRex.IO.Helpers
{
  public static class GenericArrayPoolCacheHelper<T>
  {
    private static IGenericArrayPoolCaches<T> _caches;

    public static IGenericArrayPoolCaches<T> Caches() 
    {
      if (_caches == null)
      {
        _caches = DIContext.Obtain<IGenericArrayPoolCaches<T>>() ?? new GenericArrayPoolCaches<T>();
        GenericArrayPoolCachesRegister.Add(_caches);
      }

      return _caches;
    }

    public static void Clear()
    {
      GenericArrayPoolCachesRegister.Remove(_caches);

      _caches = null;
    }
  }
}
