namespace VSS.TRex.IO
{
  /// <summary>
  /// Implements a pool of small arrays that are allocated together into a single allocated slab
  /// and represented by TRexSpan instances
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class SlabAllocatedPool<T>
  {
    public readonly int PoolSize;
    public readonly int ArraySize;
    public readonly int SpanCount;

    private T[] _slab;
    private TRexSpan<T>[] _arrays;

    private int _availCount;
    public int AvailCount => _availCount;

    public SlabAllocatedPool(int poolSize, int arraySize)
    {
      PoolSize = poolSize;
      ArraySize = arraySize;

      // Create a single allocation to contain a slab of elements of size pool size
      _slab = new T[PoolSize];

      SpanCount = PoolSize / ArraySize;
      _availCount = SpanCount;

      // Create an array of sub array spans that fit within the overall slab
      _arrays = new TRexSpan<T>[_availCount];
      for (int i = 0, limit = _arrays.Length; i < limit; i++)
      {
        _arrays[i] = new TRexSpan<T>(_slab, i * ArraySize, ArraySize);
      }
    }

    public TRexSpan<T> Rent()
    {
      if (_availCount == 0)
      {
        // The pool is empty. Synthesize a new span and return it. This span will be discarded when returned
        return new TRexSpan<T>(new T[ArraySize], 0, ArraySize, false);
      }

      lock (_arrays)
      { 
        return _arrays[--_availCount];
      }
    }

    public void Return(TRexSpan<T> buffer)
    {
      lock (_arrays)
      {
        //if (_availCount < SpanCount)
        {
          _arrays[_availCount++] = buffer;
        }
      }
    }
  }
}
