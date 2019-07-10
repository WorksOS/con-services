using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.SiteModelChangeMaps.Interfaces;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues;

namespace VSS.TRex.SiteModelChangeMaps.GridFabric.Queues
{
  /// <summary>
  /// Represents the state of a TAG file stored in the TAG file buffer queue awaiting processing.
  /// </summary>
  public class SiteModelChangeBufferQueueItem : IBinarizable, IFromToBinary, ISiteModelChangeBufferQueueItem
  { 
    public const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The date at which the item was inserted into the buffer queue. This field is indexed to permit
    /// processing items in the order they arrived
    /// </summary>
    public DateTime InsertUTC { get; set; }

    /// <summary>
    /// The contents of the site model change, as a byte array
    /// </summary>
    public byte[] Content { get; set; }

    /// <summary>
    /// UID identifier of the project this change map relates to
    /// This field is used as the affinity key map that determines which mutable server will
    /// store this TAG file.
    /// </summary>
    public Guid ProjectUID { get; set; }

    /// <summary>
    /// The type of operation to be performed between the change map content in this item and the
    /// destination change map maintained for a machine in a project
    /// </summary>
    public SiteModelChangeMapOperation Operation { get; set; }

    /// <summary>
    /// The origin of the change map delta represented by this item, such as production data ingest
    /// or query processing 
    /// </summary>
    public SiteModelChangeMapOrigin Origin { get; set; }

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteLong(InsertUTC.ToBinary());
      writer.WriteByteArray(Content);
      writer.WriteGuid(ProjectUID);
      writer.WriteInt((int)Operation);
      writer.WriteInt((int)Origin);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      InsertUTC = DateTime.FromBinary(reader.ReadLong());
      Content = reader.ReadByteArray();
      ProjectUID = reader.ReadGuid() ?? Guid.Empty;
      Operation = (SiteModelChangeMapOperation) reader.ReadInt();
      Origin = (SiteModelChangeMapOrigin) reader.ReadInt();
    }
  }
}
