using System;
using System.IO;
using Apache.Ignite.Core.Transactions;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Storage.Interfaces
{
  public interface IStorageProxy
  {
    IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper> NonSpatialCache(FileSystemStreamType streamType);
    IStorageProxyCache<ISubGridSpatialAffinityKey, ISerialisedByteArrayWrapper> SpatialCache(FileSystemStreamType streamType);
    IStorageProxyCache<ISiteModelMachineAffinityKey, ISerialisedByteArrayWrapper> ProjectMachineCache(FileSystemStreamType streamType);

    StorageMutability Mutability { get; set; }

    FileSystemErrorStatus WriteStreamToPersistentStore(Guid dataModelId,
      string streamName,
      FileSystemStreamType streamType,
      MemoryStream mutableStream,
      object source);

    FileSystemErrorStatus WriteSpatialStreamToPersistentStore(Guid dataModelId,
      string streamName,
      int subGridX, int subGridY,
      long segmentStartDateTicks, 
      long segmentEndDateTicks,
      long version,
      FileSystemStreamType streamType,
      MemoryStream mutableStream,
      object source);

    FileSystemErrorStatus ReadStreamFromPersistentStore(Guid dataModelId,
      string streamName,
      FileSystemStreamType streamType,
      out MemoryStream stream);

    FileSystemErrorStatus ReadSpatialStreamFromPersistentStore(Guid dataModelId,
      string streamName,
      int subGridX, int subGridY,
      long segmentStartDateTicks, 
      long segmentEndDateTicks,
      long version,
      FileSystemStreamType streamType,
      out MemoryStream stream);

    FileSystemErrorStatus RemoveStreamFromPersistentStore(Guid dataModelId,
      FileSystemStreamType streamType,
      string streamName);

    FileSystemErrorStatus RemoveSpatialStreamFromPersistentStore(Guid dataModelId,
      string streamName,
      int subGridX, int subGridY,
      long segmentStartDateTicks,
      long segmentEndDateTicks,
      long version,
      FileSystemStreamType streamType);

    void SetImmutableStorageProxy(IStorageProxy immutableProxy);

    IStorageProxy ImmutableProxy { get; }

    ITransaction StartTransaction(TransactionConcurrency concurrency, TransactionIsolation isolation);

    bool Commit();

    bool Commit(ITransaction tx);

    bool Commit(out int numDeleted, out int numUpdated, out long numBytesWritten);

    bool Commit(ITransaction tx, out int numDeleted, out int numUpdated, out long numBytesWritten);

    void Clear();

    long PotentialCommitWrittenBytes();
  }
}
