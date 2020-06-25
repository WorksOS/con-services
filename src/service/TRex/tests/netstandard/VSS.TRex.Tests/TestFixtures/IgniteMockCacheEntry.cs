using Apache.Ignite.Core.Cache;

namespace VSS.TRex.Tests.TestFixtures
{
  public class IgniteMockCacheEntry<TK, TV> : ICacheEntry<TK, TV>
  {
    public TK Key { get; }
    public TV Value { get; }

    public IgniteMockCacheEntry(TK key, TV value)
    {
      Key = key;
      Value = value;
    }
  }
}
