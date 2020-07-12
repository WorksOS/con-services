namespace VSS.TRex.IO.Helpers
{
  public static class GenericSlabAllocatedArrayPoolHelper<T>
  {
    private static readonly object lockObj = new object();

    private static ISlabAllocatedArrayPool<T> _pool;

    public static ISlabAllocatedArrayPool<T> Caches()
    {
      if (_pool == null)
      {
        lock (lockObj)
        {
          if (_pool == null)
          {
            _pool = new SlabAllocatedArrayPool<T>();
            GenericSlabAllocatedArrayPoolRegister.Add(_pool);
          }
        }
      }

      return _pool;
    }

    public static void Clear() => _pool = null;
  }
}
