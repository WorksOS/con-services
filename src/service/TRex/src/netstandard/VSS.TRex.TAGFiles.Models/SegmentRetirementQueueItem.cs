using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.TAGFiles.Models
{
  /// <summary>
  /// Represents a segment that has been stored in the persistent layer as a result on TAG file processing that
  /// has subsequently been updated with a later TAG file generated update.
  /// </summary>
  public class SegmentRetirementQueueItem : IBinarizable, IFromToBinary
  {
    private static ISubGridSpatialAffinityKeyFactory KeyFactory = DIContext.Obtain<ISubGridSpatialAffinityKeyFactory>();

    private const byte VERSION_NUMBER = 1;
    /// <summary>
    /// The project this segment retirement queue item refers to
    /// </summary>
    public Guid ProjectUID;

    /// <summary>
    /// The date at which the segment to be retired was inserted into the buffer queue. 
    /// </summary>
//    [QuerySqlField(IsIndexed = true)]
    public long InsertUTCAsLong;

    /// <summary>
    /// The list of keys of the subgrid and segment streams to be retired.
    /// This list is submitted as a single collection of retirement items per integration update epoch in the TAG file processor
    /// </summary>
    public ISubGridSpatialAffinityKey[] SegmentKeys;

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());
    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(VERSION_NUMBER);
      writer.WriteGuid(ProjectUID);
      writer.WriteLong(InsertUTCAsLong);

      writer.WriteBoolean(SegmentKeys != null);
      if (SegmentKeys != null)
      {
        writer.WriteInt(SegmentKeys.Length);

        for (int i = 0; i < SegmentKeys.Length; i++)
          ((IFromToBinary) SegmentKeys[i]).ToBinary(writer);
      }
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      var version = reader.ReadByte();

      if (version != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, version);

      ProjectUID = reader.ReadGuid() ?? Guid.Empty;
      InsertUTCAsLong = reader.ReadLong();

      if (reader.ReadBoolean())
      {
        int numKeys = reader.ReadInt();
        SegmentKeys = new ISubGridSpatialAffinityKey[numKeys];

        for (int i = 0; i < numKeys; i++)
        {
          SegmentKeys[i] = KeyFactory.NewInstance();
          ((IFromToBinary) SegmentKeys[i]).FromBinary(reader);
        }
      }
    }
  }
}
