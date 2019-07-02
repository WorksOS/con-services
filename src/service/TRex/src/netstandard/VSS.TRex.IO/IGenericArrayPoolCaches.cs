namespace VSS.TRex.IO
{
  public interface IGenericArrayPoolCaches
  {
    /// <summary>
    /// Supplies statistics on the usage of the cached array pools
    /// </summary>
    /// <returns></returns>
    GenericArrayPoolStatistics[] Statistics();

    string ToString();

    string TypeName();

    /// <summary>
    /// Resets counts of all exponential buckets to zero, clearing the cache content as a result.
    /// </summary>
    void Clear();
  }

  public interface IGenericArrayPoolCaches<T> : IGenericArrayPoolCaches
  {
    /// <summary>
    /// Rents out a buffer from the pool. The buffer is greater than or equal to the size of the requested buffer.
    /// Pooled buffers are created on demand - each pool is not initialized with a full set of buffers.
    /// Note: The pool does not maintain a reference to the buffer. If tha renter fails to return the buffer pool
    /// it will be cleaned up by the garbage collector.
    /// </summary>
    /// <param name="minSize"></param>
    /// <returns></returns>
    T[] Rent(int minSize);

    /// <summary>
    /// Returns a previously rented buffer from the pool.
    /// Note: If the size of the returned buffer is not a power of two it will be logged but otherwise ignored.
    /// </summary>
    /// <param name="buffer"></param>
    void Return(ref T[] buffer);
  }
}
