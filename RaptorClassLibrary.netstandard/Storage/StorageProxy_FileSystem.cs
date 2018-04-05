using System;
using System.IO;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Storage
{
    /// <summary>
    /// Implementation of the IStorageProxy interface that allows to read/write operations against Raptor IO Service
    /// </summary>
    public class StorageProxy_FileSystem : IStorageProxy
    {
        public StorageMutability Mutability { get; set; } = StorageMutability.Immutable;

        public StorageProxy_FileSystem()
        {
        }

        FileSystemErrorStatus IStorageProxy.ReadSpatialStreamFromPersistentStore(long DataModelID, string StreamName, uint SubgridX, uint SubgridY, string segmentIdentifier, FileSystemStreamType StreamType, uint GranuleIndex, out MemoryStream Stream, out uint StoreGranuleIndex, out uint StoreGranuleCount)
        {
            throw new NotImplementedException();
        }

        FileSystemErrorStatus IStorageProxy.ReadStreamFromPersistentStore(long DataModelID, string StreamName, FileSystemStreamType StreamType, out MemoryStream Stream)
        {
            throw new NotImplementedException();
        }

        FileSystemErrorStatus IStorageProxy.ReadStreamFromPersistentStore(long DataModelID, string StreamName, FileSystemStreamType StreamType, out MemoryStream Streamout, out uint StoreGranuleIndex, out uint StoreGranuleCount)
        {
            throw new NotImplementedException();
        }

        FileSystemErrorStatus IStorageProxy.ReadStreamFromPersistentStoreDirect(long DataModelID, string StreamName, FileSystemStreamType StreamType, out MemoryStream Stream)
        {
            throw new NotImplementedException();
        }

        FileSystemErrorStatus IStorageProxy.RemoveStreamFromPersistentStore(long DataModelID, string StreamName)
        {
            throw new NotImplementedException();
        }

        FileSystemErrorStatus IStorageProxy.WriteSpatialStreamToPersistentStore(long DataModelID, string StreamName, uint SubgridX, uint SubgridY, string SegmentIdentifier, FileSystemStreamType StreamType, out uint StoreGranuleIndex, out uint StoreGranuleCount, MemoryStream Stream)
        {
            throw new NotImplementedException();
        }

        FileSystemErrorStatus IStorageProxy.WriteStreamToPersistentStore(long DataModelID, string StreamName, FileSystemStreamType StreamType, out uint StoreGranuleIndex, out uint StoreGranuleCount, MemoryStream Stream)
        {
            throw new NotImplementedException();
        }

        FileSystemErrorStatus IStorageProxy.WriteStreamToPersistentStoreDirect(long DataModelID, string StreamName, FileSystemStreamType StreamType, MemoryStream Stream)
        {
            throw new NotImplementedException();
        }

        public void SetImmutableStorageProxy(IStorageProxy immutableProxy)
        {
            throw new NotImplementedException();
        }
    }
}
