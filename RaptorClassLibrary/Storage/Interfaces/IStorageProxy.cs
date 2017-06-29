using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Interfaces
{
    public interface IStorageProxy
    {
        FileSystemErrorStatus WriteStreamToPersistentStore(long DataModelID,
                                              string StreamName,
                                              FileSystemGranuleType StreamType,
                                              out uint StoreGranuleIndex,
                                              out uint StoreGranuleCount,
                                              MemoryStream Stream);

        FileSystemErrorStatus WriteStreamToPersistentStoreDirect(long DataModelID,
                                              string StreamName,
                                              FileSystemGranuleType StreamType,
                                                MemoryStream Stream);

        FileSystemErrorStatus WriteSpatialStreamToPersistentStore(long DataModelID,
                                              string StreamName,
                                              uint SubgridX, uint SubgridY,
                                              string SegmentIdentifier,
                                              // Don't implement yet.....
                                              // const AInvalidatedSpatialStreams : TInvalidatedSpatialStreamArray;
                                              FileSystemStreamType StreamType,
                                              out uint StoreGranuleIndex,
                                              out uint StoreGranuleCount,
                                              MemoryStream Stream);

        FileSystemErrorStatus ReadStreamFromPersistentStore(long DataModelID,
                                              string StreamName,
                                              FileSystemStreamType StreamType,
                                              out MemoryStream Streamout,
                                              uint StoreGranuleIndex,
                                              out uint StoreGranuleCount);
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
                                                  uint GranuleIndex,
                                                  out MemoryStream Stream,
                                                  out uint StoreGranuleIndex,
                                                  out uint StoreGranuleCount);

        FileSystemErrorStatus RemoveStreamFromPersistentStore(long DataModelID,
                                                              string StreamName);
        /*
            function CopyDataModel(const DataModelID : Int64; const DestinationFileName: String): TICFSErrorStatus;
            function SwapDataModel(const DataModelID : Int64; const SourceFileName: String): TICFSErrorStatus;
            function ChangeDataModelState(const DataModelID : Int64; const Operation: Integer): TICFSErrorStatus;
            function ReportDataModelState(const DataModelID : Int64; var Status : TICFSClosingStatus): TICFSErrorStatus;
        */
    }
}
