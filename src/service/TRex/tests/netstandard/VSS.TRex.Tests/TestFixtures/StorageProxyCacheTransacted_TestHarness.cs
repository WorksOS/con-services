using System.Collections.Generic;
using Apache.Ignite.Core.Cache;
using VSS.TRex.Storage;

namespace VSS.TRex.Tests.TestFixtures
{
  public class StorageProxyCacheTransacted_TestHarness<TK, TV> : StorageProxyCacheTransacted<TK, TV>, IStorageProxyCacheTransacted_TestHarness<TK, TV>
  {
    public StorageProxyCacheTransacted_TestHarness(ICache<TK, TV> cache) : base(cache)
    {
    }

    /// <summary>
    /// Override the commit behaviour to make it a null operation for unit test activities
    /// </summary>
    public override void Commit(out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      // Do nothing on purpose
      numDeleted = 0;
      numUpdated = 0;
      numBytesWritten = 0;
    }

    /// <summary>
    /// Override the look-aside get semantics into the transaction writes so that gets don't read through into the
    /// null cache reference in the underlying base class.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public override TV Get(TK key)
    {
      return PendingTransactedWrites.TryGetValue(key, out TV value) ? value : throw new KeyNotFoundException($"Key {key} not found");
    }
  }
}
