namespace VSS.TRex.IO
{
  public interface ISlabAllocatedArrayPool
  {
    /// <summary>
    /// Provides a vector of statistics on the pools in the slab allocated pool array
    /// </summary>
    /// <returns></returns>
    (int PoolIndex, int ArraySize, int Capacity, int RentedItems)[] Statistics();

    void Clear();

    string TypeName();
  }

  public interface ISlabAllocatedArrayPool<T> : ISlabAllocatedArrayPool
  {
    /// <summary>
    /// Rents out a buffer from the pool. The buffer is greater than or equal to the size of the requested buffer.
    /// Pooled buffers are created on demand - each pool is not initialized with a full set of buffers.
    /// Note: The pool does not maintain a reference to the buffer. If tha renter fails to return the buffer pool
    /// it will be cleaned up by the garbage collector.
    /// </summary>
    /// <param name="minSize"></param>
    /// <returns></returns>
    TRexSpan<T> Rent(int minSize);

    /// <summary>
    /// Returns a previously rented buffer from the pool.
    /// Note: If the size of the returned buffer is not a power of two it will be logged but otherwise ignored.
    /// </summary>
    /// <param name="buffer"></param>
    void Return(ref TRexSpan<T> buffer);

    /// <summary>
    /// Clone the contents in 'oldBuffer' and return a new TRexSPan containing the cloned content
    /// </summary>
    /// <param name="oldBuffer"></param>
    /// <returns></returns>
    TRexSpan<T> Clone(TRexSpan<T> oldBuffer);
  }
}
