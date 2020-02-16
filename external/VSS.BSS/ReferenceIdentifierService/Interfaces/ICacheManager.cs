namespace VSS.Nighthawk.ReferenceIdentifierService.Interfaces
{
  public interface ICacheManager
  {
    void Add(string key, object value, int cacheLifetimeMinutes);
    bool Contains(string key);
    object GetData(string key);
    object GetClosestData(string key);
    void Remove(string key);
  }
}
