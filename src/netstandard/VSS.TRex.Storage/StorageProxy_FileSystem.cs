using System;
using System.IO;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Storage
{
    /// <summary>
    /// Implementation of the IStorageProxy interface that allows to read/write operations against TRex IO Service
    /// </summary>
    public class StorageProxy_FileSystem : IStorageProxy
    {
        public StorageMutability Mutability { get; set; } = StorageMutability.Immutable;

        public StorageProxy_FileSystem()
        {
        }

        FileSystemErrorStatus IStorageProxy.ReadSpatialStreamFromPersistentStore(Guid DataModelID, string StreamName, uint SubgridX, uint SubgridY, string segmentIdentifier, FileSystemStreamType StreamType, out MemoryStream Stream)
        {
            throw new NotImplementedException();
        }

        FileSystemErrorStatus IStorageProxy.ReadStreamFromPersistentStore(Guid DataModelID, string StreamName, FileSystemStreamType StreamType, out MemoryStream Stream)
        {
            throw new NotImplementedException();
        }

        FileSystemErrorStatus IStorageProxy.ReadStreamFromPersistentStoreDirect(Guid DataModelID, string StreamName, FileSystemStreamType StreamType, out MemoryStream Stream)
        {
            throw new NotImplementedException();
        }

        FileSystemErrorStatus IStorageProxy.RemoveStreamFromPersistentStore(Guid DataModelID, string StreamName)
        {
            throw new NotImplementedException();
        }

        FileSystemErrorStatus IStorageProxy.WriteSpatialStreamToPersistentStore(Guid DataModelID, string StreamName, uint SubgridX, uint SubgridY, string SegmentIdentifier, FileSystemStreamType StreamType, MemoryStream Stream)
        {
            throw new NotImplementedException();
        }

        FileSystemErrorStatus IStorageProxy.WriteStreamToPersistentStore(Guid DataModelID, string StreamName, FileSystemStreamType StreamType, MemoryStream Stream)
        {
            throw new NotImplementedException();
        }

        FileSystemErrorStatus IStorageProxy.WriteStreamToPersistentStoreDirect(Guid DataModelID, string StreamName, FileSystemStreamType StreamType, MemoryStream Stream)
        {
            throw new NotImplementedException();
        }

        public void SetImmutableStorageProxy(IStorageProxy immutableProxy)
        {
            throw new NotImplementedException();
        }

        public bool Commit()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
