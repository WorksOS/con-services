using System;

namespace VSS.TRex.IO
{
  public interface IGenericTwoDArrayCache
  {
    (int currentSize, int maxSize) Statistics();

    string TypeName();

    /// <summary>
    /// Resets counts of all buckets to zero, clearing the cache content as a result.
    /// </summary>
    void Clear();
  }

  public interface IGenericTwoDArrayCache<T> : IGenericTwoDArrayCache
  {
    T[,] Rent();
    T[,] RentEx(Action<T[,]> validator);
    void Return(ref T[,] value);
  }
}
