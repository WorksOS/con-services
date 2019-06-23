namespace VSS.TRex.IO
{
  /// <summary>
  /// Defines a page of elements of type T. Typically this page will be relatively small, and
  /// should not be greater than 65536 elements to keep the span offset value in a
  /// ushort numeric type.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class SlabAllocatedPoolPage<T>
  {
    private readonly T[] _slabPage;
    public readonly TRexSpan<T>[] Arrays;

    public SlabAllocatedPoolPage(int poolSize, int arraySize)
    {
      // Create a single allocation to contain a slab of elements of size pool size
      _slabPage = new T[poolSize];

      int spanCount = poolSize / arraySize;

      // Create an array of sub array spans that fit within the overall slab
      Arrays = new TRexSpan<T>[spanCount];

      for (int i = 0; i < spanCount; i++)
      {
        Arrays[i] = new TRexSpan<T>(_slabPage, 0, (ushort)(i * arraySize), arraySize,  true);
      }
    }
  }
}
