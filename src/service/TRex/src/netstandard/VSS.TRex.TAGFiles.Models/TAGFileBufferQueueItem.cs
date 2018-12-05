using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.TAGFiles.Models
{
  /// <summary>
  /// Represents the state of a TAG file stored in the TAG file buffer queue awaiting processing.
  /// </summary>
  public class TAGFileBufferQueueItem : IBinarizable, IFromToBinary
  { 
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The date at which the TAG file was inserted into the buffer queue. This field is indexed to permit
    /// processing TAG files in the order they arrived
    /// </summary>
    public DateTime InsertUTC;

    /// <summary>
    /// The original filename for the TAG file
    /// </summary>
    public string FileName;

    /// <summary>
    /// The contents of the TAG file, as a byte array
    /// </summary>
    public byte[] Content;

    /// <summary>
    /// UID identifier of the project to process this TAG file into.
    /// This field is used as the affinity key map that determines which mutable server will
    /// store this TAG file.
    /// </summary>
    public Guid ProjectID;

    /// <summary>
    /// UID identifier of the asset to process this TAG file into
    /// </summary>
    public Guid AssetID;

    /// <summary>
    ///   Is machine a JohnDoe. No telematics device on board to identify machine or No AssetUID in system
    ///   JohnDoe machine are assigned a unique Guid
    /// </summary>
    public bool IsJohnDoe;

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());


    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(VERSION_NUMBER);

      writer.WriteLong(InsertUTC.ToBinary());
      writer.WriteString(FileName);
      writer.WriteByteArray(Content);
      writer.WriteGuid(ProjectID);
      writer.WriteGuid(AssetID);
      writer.WriteBoolean(IsJohnDoe);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      var version = reader.ReadByte();

      if (version != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, version);

      InsertUTC = DateTime.FromBinary(reader.ReadLong());
      FileName = reader.ReadString();
      Content = reader.ReadByteArray();
      ProjectID = reader.ReadGuid() ?? Guid.Empty;
      AssetID = reader.ReadGuid() ?? Guid.Empty;
      IsJohnDoe = reader.ReadBoolean();
    }
  }
}
