using Apache.Ignite.Core.Transactions;

namespace VSS.TRex.Storage.Interfaces
{
  public interface IStorageProxyCacheCommit
  {
    void Commit();

    void Commit(ITransaction tx);

    void Commit(out int numDeleted, out int numUpdated, out long numBytesWritten);


    void Commit(ITransaction tx, out int numDeleted, out int numUpdated, out long numBytesWritten);
    void IncrementBytesWritten(long bytesWritten);
    
    string Name { get; }

    void Clear();
  }
}
