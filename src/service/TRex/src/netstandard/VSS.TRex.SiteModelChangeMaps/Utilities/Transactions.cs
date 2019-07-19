using Apache.Ignite.Core.Transactions;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.SiteModelChangeMaps.Utilities
{
  public static class Transactions
  {
    /// <summary>
    /// Creates a new transaction suitable for transactional control over change maps information
    /// </summary>
    /// <param name="storageProxy"></param>
    /// <returns></returns>
    public static ITransaction StartTransaction(IStorageProxy storageProxy)
    {
      return storageProxy.StartTransaction(SiteModelChangeMapsConsts.ChangeMapTxConcurrency, 
                                           SiteModelChangeMapsConsts.ChangeMapTxIsolation);
    }
  }
}
