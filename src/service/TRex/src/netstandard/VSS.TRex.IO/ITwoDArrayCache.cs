namespace VSS.TRex.IO
{
  public interface ITwoDArrayCache<T>
  {
    T[,] Rent();
    void Return(T[,] value);
    (int currentSize, int maxSize) Statistics();
  }
}
