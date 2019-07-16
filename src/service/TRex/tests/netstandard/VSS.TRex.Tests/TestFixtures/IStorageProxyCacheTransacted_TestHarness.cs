using System.Collections.Generic;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Tests.TestFixtures
{
  public interface IStorageProxyCacheTransacted_TestHarness<TK, TV> : IStorageProxyCacheTransacted<TK, TV>
  {
    Dictionary<TK, TV> GetPendingTransactedWrites();
  }
}
