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
    public readonly T[] SlabPage;
    public readonly TRexSpan<T>[] Arrays;

    public SlabAllocatedPoolPage(int poolSize, int arraySize)
    {
      // Create a single allocation to contain a slab of elements of size pool size
      SlabPage = new T[poolSize];

      int spanCount = poolSize / arraySize;

      // Create an array of sub array spans that fit within the overall slab
      Arrays = new TRexSpan<T>[spanCount];

      for (int i = 0; i < spanCount; i++)
      {
        Arrays[i] = new TRexSpan<T>(SlabPage, 0, (ushort)(i * arraySize), arraySize,  true);
      }
    }
  }
}
