using Apache.Ignite.Core.Transactions;

namespace VSS.TRex.SiteModelChangeMaps
{
  public static class SiteModelChangeMapsConsts
  {
    /// <summary>
    /// The transaction concurrency level for transactions managing access to change maps
    /// This is set to Pessimistic to ensure locks are acquired on first read of an item
    /// </summary>
    public static TransactionConcurrency ChangeMapTxConcurrency = TransactionConcurrency.Pessimistic;

    /// <summary>
    /// The transaction isolation level for  transactions managing access to change maps
    /// This is set to repeatable read as pessimistic concurrency will acquire a lock on the first read
    /// </summary>
    public static TransactionIsolation ChangeMapTxIsolation = TransactionIsolation.RepeatableRead;
  }
}
