using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Storage.Interfaces
{
    public interface IStorageProxy
    {
        IStorageProxyCache<INonSpatialAffinityKey, byte[]> NonSpatialCache { get; }
        IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]> SpatialCache { get; }

        StorageMutability Mutability { get; set; }

        FileSystemErrorStatus WriteStreamToPersistentStore(Guid DataModelID,
                                              string StreamName,
                                              FileSystemStreamType StreamType,
                                              MemoryStream Stream);

        FileSystemErrorStatus WriteSpatialStreamToPersistentStore(Guid DataModelID,
                                              string StreamName,
                                              uint SubgridX, uint SubgridY,
                                              string SegmentIdentifier,
                                              FileSystemStreamType StreamType,
                                              MemoryStream Stream);

        FileSystemErrorStatus ReadStreamFromPersistentStore(Guid DataModelID,
                                                  string StreamName,
                                                  FileSystemStreamType StreamType,
                                                  out MemoryStream Stream);

        FileSystemErrorStatus ReadSpatialStreamFromPersistentStore(Guid DataModelID,
                                                  string StreamName,
                                                  uint SubgridX, uint SubgridY,
                                                  string SegmentIdentifier,
                                                  FileSystemStreamType StreamType,
                                                  out MemoryStream Stream);

        FileSystemErrorStatus RemoveStreamFromPersistentStore(Guid DataModelID,
                                                              string StreamName);

        void SetImmutableStorageProxy(IStorageProxy immutableProxy);

        IStorageProxy ImmutableProxy { get; }


        bool Commit();
        void Clear();

        /*
            function CopyDataModel(const DataModelID : Int64; const DestinationFileName: String): TICFSErrorStatus;
            function SwapDataModel(const DataModelID : Int64; const SourceFileName: String): TICFSErrorStatus;
            function ChangeDataModelState(const DataModelID : Int64; const Operation: Integer): TICFSErrorStatus;
            function ReportDataModelState(const DataModelID : Int64; var Status : TICFSClosingStatus): TICFSErrorStatus;
        */
    }
}
