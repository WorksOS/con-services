
namespace VSS.TRex.Caching.Interfaces
{
  public interface IMRURingBuffer<T>
  {
    long Put(T element);
    T Get(ref long token);
    T Remove(long token);
  }
}
