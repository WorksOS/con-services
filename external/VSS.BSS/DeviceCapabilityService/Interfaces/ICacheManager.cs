namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces
{
  public interface ICacheManager
  {
    void Add(string key, object value, int cacheLifetimeMinutes);

    bool Contains(string key);

    object GetData(string key);

    void Remove(string key);
  }
}
