using System.IO;
using VSS.VisionLink.Raptor.Storage;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Interfaces
{
    public interface IStorageProxy
    {
        StorageMutability Mutability { get; set; }

        FileSystemErrorStatus WriteStreamToPersistentStore(long DataModelID,
                                              string StreamName,
                                              FileSystemStreamType StreamType,
                                              MemoryStream Stream);

        FileSystemErrorStatus WriteStreamToPersistentStoreDirect(long DataModelID,
                                              string StreamName,
                                              FileSystemStreamType StreamType,
                                              MemoryStream Stream);

        FileSystemErrorStatus WriteSpatialStreamToPersistentStore(long DataModelID,
                                              string StreamName,
                                              uint SubgridX, uint SubgridY,
                                              string SegmentIdentifier,
                                              // Don't implement yet.....
                                              // const AInvalidatedSpatialStreams : TInvalidatedSpatialStreamArray;
                                              FileSystemStreamType StreamType,
                                              MemoryStream Stream);

        FileSystemErrorStatus ReadStreamFromPersistentStore(long DataModelID,
                                                  string StreamName,
                                                  FileSystemStreamType StreamType,
                                                  out MemoryStream Stream);

        FileSystemErrorStatus ReadStreamFromPersistentStoreDirect(long DataModelID,
                                                  string StreamName,
                                                  FileSystemStreamType StreamType,
                                                  out MemoryStream Stream);

        FileSystemErrorStatus ReadSpatialStreamFromPersistentStore(long DataModelID,
                                                  string StreamName,
                                                  uint SubgridX, uint SubgridY,
                                                  string SegmentIdentifier,
                                                  FileSystemStreamType StreamType,
                                                  out MemoryStream Stream);

        FileSystemErrorStatus RemoveStreamFromPersistentStore(long DataModelID,
                                                              string StreamName);

        void SetImmutableStorageProxy(IStorageProxy immutableProxy);

        bool Commit();

        /*
            function CopyDataModel(const DataModelID : Int64; const DestinationFileName: String): TICFSErrorStatus;
            function SwapDataModel(const DataModelID : Int64; const SourceFileName: String): TICFSErrorStatus;
            function ChangeDataModelState(const DataModelID : Int64; const Operation: Integer): TICFSErrorStatus;
            function ReportDataModelState(const DataModelID : Int64; var Status : TICFSClosingStatus): TICFSErrorStatus;
        */
    }
}
