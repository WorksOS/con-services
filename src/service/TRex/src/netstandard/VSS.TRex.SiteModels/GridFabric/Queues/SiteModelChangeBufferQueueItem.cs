using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.SiteModels.GridFabric.Queues
{
  /// <summary>
  /// Represents the state of a TAG file stored in the TAG file buffer queue awaiting processing.
  /// </summary>
  public class SiteModelChangeBufferQueueItem : IBinarizable, IFromToBinary
  { 
    public const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The date at which the item was inserted into the buffer queue. This field is indexed to permit
    /// processing items in the order they arrived
    /// </summary>
    public DateTime InsertUTC;

    /// <summary>
    /// The contents of the site model change, as a byte array
    /// </summary>
    public byte[] Content;

    /// <summary>
    /// UID identifier of the project this change map relates to
    /// This field is used as the affinity key map that determines which mutable server will
    /// store this TAG file.
    /// </summary>
    public Guid ProjectUID;

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteLong(InsertUTC.ToBinary());
      writer.WriteByteArray(Content);
      writer.WriteGuid(ProjectUID);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      InsertUTC = DateTime.FromBinary(reader.ReadLong());
      Content = reader.ReadByteArray();
      ProjectUID = reader.ReadGuid() ?? Guid.Empty;
    }
  }
}
