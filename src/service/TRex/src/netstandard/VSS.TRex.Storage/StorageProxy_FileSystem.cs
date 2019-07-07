using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Apache.Ignite.Core.Transactions;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Storage
{
  /// <summary>
  /// Implementation of the IStorageProxy interface that allows to read/write operations against TRex IO Service
  /// </summary>
  [ExcludeFromCodeCoverage] // Not currently used...
  public class StorageProxy_FileSystem : IStorageProxy
  {
    public IStorageProxyCache<INonSpatialAffinityKey, byte[]> NonSpatialCache(FileSystemStreamType streamType) => null; // Not implemented
    public IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]> SpatialCache { get; } = null; // Not implemented

    public StorageMutability Mutability { get; set; } = StorageMutability.Immutable;

    public StorageProxy_FileSystem()
    {
    }

    FileSystemErrorStatus IStorageProxy.WriteStreamToPersistentStore(Guid dataModelID, string streamName, FileSystemStreamType streamType,
      MemoryStream mutablestream, object source)
    {
      throw new NotImplementedException();
    }

    FileSystemErrorStatus IStorageProxy.WriteSpatialStreamToPersistentStore(Guid dataModelID, string streamName, int subGridX, int subGridY, long segmentStartDateTicks, long segmentEndDateTicks,
      long version, FileSystemStreamType streamType, MemoryStream mutableStream, object source)
    {
      throw new NotImplementedException();
    }

    FileSystemErrorStatus IStorageProxy.ReadStreamFromPersistentStore(Guid dataModelID, string streamName, FileSystemStreamType streamType, out MemoryStream stream)
    {
      throw new NotImplementedException();
    }

    FileSystemErrorStatus IStorageProxy.ReadSpatialStreamFromPersistentStore(Guid dataModelID, string streamName, int subGridX, int subGridY, long segmentStartDateTicks, long segmentEndDateTicks, long version, FileSystemStreamType streamType, out MemoryStream stream)
    {
      throw new NotImplementedException();
    }

    FileSystemErrorStatus IStorageProxy.RemoveStreamFromPersistentStore(Guid dataModelID, FileSystemStreamType streamType, string streamName)
    {
      throw new NotImplementedException();
    }

    public void SetImmutableStorageProxy(IStorageProxy immutableProxy)
    {
      throw new NotImplementedException();
    }

    public IStorageProxy ImmutableProxy { get; }

    public bool Commit()
    {
      throw new NotImplementedException();
    }

    public bool Commit(out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      throw new NotImplementedException();
    }

    public void Clear()
    {
      throw new NotImplementedException();
    }

    public IStorageProxyCache<ISiteModelMachineAffinityKey, byte[]> ProjectMachineCache(FileSystemStreamType streamType)
    {
      throw new NotImplementedException();
    }

    public bool Commit(ITransaction tx, out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      throw new NotImplementedException();
    }

    public bool Commit(ITransaction tx)
    {
      throw new NotImplementedException();
    }

    public ITransaction StartTransaction()
    {
      throw new NotImplementedException();
    }
  }
}
