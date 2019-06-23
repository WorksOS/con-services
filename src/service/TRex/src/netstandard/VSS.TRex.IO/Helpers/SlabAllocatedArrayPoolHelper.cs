using VSS.TRex.DI;

namespace VSS.TRex.IO.Helpers
{
  public static class SlabAllocatedArrayPoolHelper<T>
  {
    private static ISlabAllocatedArrayPool<T> _caches;
   public static ISlabAllocatedArrayPool<T> Caches => _caches ?? (_caches = DIContext.Obtain<ISlabAllocatedArrayPool<T>>());

    
    public static void Clear() => _caches = null;
  }
}
