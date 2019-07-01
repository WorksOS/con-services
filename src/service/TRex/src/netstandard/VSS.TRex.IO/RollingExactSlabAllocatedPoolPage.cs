using System.Threading;

namespace VSS.TRex.IO
{
  /// <summary>
  /// Defines a page of elements of type T. Typically this page will be relatively small, and
  /// should not be greater than 65536 elements to keep the span offset value in a
  /// ushort numeric type.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class RollingExactSlabAllocatedPoolPage<T>
  {
    private readonly T[] _slabPage;
   // public readonly TRexSpan<T>[] Arrays;

    /// <summary>
    /// The high water mark allocations within this page has reached
    /// </summary>
    private int _highWaterMark = 0;

    public RollingExactSlabAllocatedPoolPage(int poolSize, int arraySize)
    {
      // Create a single allocation to contain a slab of elements of size pool size
      _slabPage = new T[poolSize];

      int spanCount = poolSize / arraySize;

      // Create an array of sub array spans that fit within the overall slab
     /*
      Arrays = new TRexSpan<T>[spanCount];
      
      for (int i = 0; i < spanCount; i++)
      {
        Arrays[i] = new TRexSpan<T>(_slabPage, 0, (ushort)(i * arraySize), arraySize,  true);
      }
      */
    }

    /// <summary>
    /// Allocates a range of elements in this slab to the caller. The response is the start index and
    /// count that were allocated. If count is not equal to capacity then this page has insufficient capacity
    /// to service the request and the client requestor should make the request to the next available rolling page.
    /// This method is thread safe and support concurrent requests for allocation
    /// </summary>
    /// <param name="capacity"></param>
    /// <param name="startIndex"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int Allocate(int capacity, out int startIndex, out int count)
    {
      //int thisHighWaterMark = Interlocked.Increment(ref _highWaterMark); ...
      startIndex = -1;
      count = -1;
      return -1;
    }
  }
}
