using System;
using System.IO;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Storage.Interfaces
{
  public interface IStorageProxy
  {
    IStorageProxyCache<INonSpatialAffinityKey, byte[]> NonSpatialCache(FileSystemStreamType streamType);
    IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]> SpatialCache { get; }
    IStorageProxyCache<ISiteModelMachineAffinityKey, byte[]> ProjectMachineCache(FileSystemStreamType streamType);

    StorageMutability Mutability { get; set; }

    FileSystemErrorStatus WriteStreamToPersistentStore(Guid dataModelID,
      string streamName,
      FileSystemStreamType streamType,
      MemoryStream mutablestream,
      object source);

    FileSystemErrorStatus WriteSpatialStreamToPersistentStore(Guid dataModelID,
      string streamName,
      int subGridX, int subGridY,
      long segmentStartDateTicks, 
      long segmentEndDateTicks,
      long version,
      FileSystemStreamType streamType,
      MemoryStream mutableStream,
      object source);

    FileSystemErrorStatus ReadStreamFromPersistentStore(Guid dataModelID,
      string streamName,
      FileSystemStreamType streamType,
      out MemoryStream stream);

    FileSystemErrorStatus ReadSpatialStreamFromPersistentStore(Guid dataModelID,
      string streamName,
      int subGridX, int subGridY,
      long segmentStartDateTicks, 
      long segmentEndDateTicks,
      long version,
      FileSystemStreamType streamType,
      out MemoryStream stream);

    FileSystemErrorStatus RemoveStreamFromPersistentStore(Guid dataModelID,
      FileSystemStreamType streamType,
      string streamName);

    void SetImmutableStorageProxy(IStorageProxy immutableProxy);

    IStorageProxy ImmutableProxy { get; }


    bool Commit();

    bool Commit(out int numDeleted, out int numUpdated, out long numBytesWritten);

    void Clear();

    /*
        function CopyDataModel(const dataModelID : Int64; const DestinationFileName: String): TICFSErrorStatus;
        function SwapDataModel(const dataModelID : Int64; const SourceFileName: String): TICFSErrorStatus;
        function ChangeDataModelState(const dataModelID : Int64; const Operation: Integer): TICFSErrorStatus;
        function ReportDataModelState(const dataModelID : Int64; var Status : TICFSClosingStatus): TICFSErrorStatus;
    */
  }
}
