using VSS.TRex.DI;

namespace VSS.TRex.IO.Helpers
{
  public static class GenericArrayPoolCacheHelper<T>
  {
    private static IGenericArrayPoolCaches<T> _caches;
    public static IGenericArrayPoolCaches<T> Caches => _caches ?? (_caches = DIContext.Obtain<IGenericArrayPoolCaches<T>>() ?? new GenericArrayPoolCaches<T>());

    public static void Clear() => _caches = null;
  }
}
