using System;
using System.IO;
using VSS.TRex.Storage;
using VSS.TRex.Types;

namespace VSS.TRex.Storage.Interfaces
{
    public interface IStorageProxy
    {
        StorageMutability Mutability { get; set; }

        FileSystemErrorStatus WriteStreamToPersistentStore(Guid DataModelID,
                                              string StreamName,
                                              FileSystemStreamType StreamType,
                                              MemoryStream Stream);

        FileSystemErrorStatus WriteStreamToPersistentStoreDirect(Guid DataModelID,
                                              string StreamName,
                                              FileSystemStreamType StreamType,
                                              MemoryStream Stream);

        FileSystemErrorStatus WriteSpatialStreamToPersistentStore(Guid DataModelID,
                                              string StreamName,
                                              uint SubgridX, uint SubgridY,
                                              string SegmentIdentifier,
                                              // Don't implement yet.....
                                              // const AInvalidatedSpatialStreams : TInvalidatedSpatialStreamArray;
                                              FileSystemStreamType StreamType,
                                              MemoryStream Stream);

        FileSystemErrorStatus ReadStreamFromPersistentStore(Guid DataModelID,
                                                  string StreamName,
                                                  FileSystemStreamType StreamType,
                                                  out MemoryStream Stream);

        FileSystemErrorStatus ReadStreamFromPersistentStoreDirect(Guid DataModelID,
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
