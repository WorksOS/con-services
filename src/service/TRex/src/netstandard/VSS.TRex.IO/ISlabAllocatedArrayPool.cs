namespace VSS.TRex.IO
{
  public interface ISlabAllocatedArrayPool<T>
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
    void Return(TRexSpan<T> buffer);

    /// <summary>
    /// Takes a buffers and return a new buffer resized to the requested size and with as many elements from the
    /// original buffer as can be copied into it.
    /// </summary>
    /// <param name="oldBuffer"></param>
    /// <param name="minSize"></param>
    /// <returns></returns>
    TRexSpan<T> Resize(TRexSpan<T> oldBuffer, int minSize);

    /// <summary>
    /// Clone the contents in 'oldBuffer' and return a new TRexSPan containing the cloned content
    /// </summary>
    /// <param name="oldBuffer"></param>
    /// <returns></returns>
    TRexSpan<T> Clone(TRexSpan<T> oldBuffer);
  }
}
