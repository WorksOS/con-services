using VSS.TRex.DI;

namespace VSS.TRex.IO.Helpers
{
  public static class SlabAllocatedArrayPoolHelper<T>
  {
    private static readonly object lockObj = new object();

    private static ISlabAllocatedArrayPool<T> _caches;

    public static ISlabAllocatedArrayPool<T> Caches()
    {
      if (_caches == null)
      {
        lock (lockObj)
        {
          if (_caches == null)
          {
            _caches = DIContext.Obtain<ISlabAllocatedArrayPool<T>>() ?? new SlabAllocatedArrayPool<T>();
          }
        }
      }

      return _caches;
    } 

    public static void Clear() => _caches = null;
  }
}
