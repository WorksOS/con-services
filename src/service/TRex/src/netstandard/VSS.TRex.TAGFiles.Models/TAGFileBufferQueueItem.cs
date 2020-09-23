using System;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;

namespace VSS.TRex.TAGFiles.Models
{
  /// <summary>
  /// Represents the state of a TAG file stored in the TAG file buffer queue awaiting processing.
  /// </summary>
  public class TAGFileBufferQueueItem : VersionCheckedBinarizableSerializationBase
  { 
    public const byte VERSION_NUMBER = 3;
    private static byte[] VERSION_NUMBERS = { 1, 2, 3 };

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

    /// <summary>
    /// States if the TAG fie should be added to the TAG file archive during processing
    /// </summary>
    public TAGFileSubmissionFlags SubmissionFlags { get; set; } = TAGFileSubmissionFlags.AddToArchive;

    /// <summary>
    /// The origin source that produced the TAG file, such as GCS900, Earthworks etc
    /// </summary>
    public TAGFileOriginSource OriginSource { get; set; } = TAGFileOriginSource.LegacyTAGFileSource;

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteLong(InsertUTC.ToBinary());
      writer.WriteString(FileName);
      writer.WriteByteArray(Content);
      writer.WriteGuid(ProjectID);
      writer.WriteGuid(AssetID);
      writer.WriteBoolean(IsJohnDoe);
      writer.WriteInt((int)SubmissionFlags);
      writer.WriteInt((int)OriginSource);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var messageVersion = VersionSerializationHelper.CheckVersionsByte(reader, VERSION_NUMBERS);

      if (messageVersion >= 1)
      {
        InsertUTC = DateTime.FromBinary(reader.ReadLong());
        FileName = reader.ReadString();
        Content = reader.ReadByteArray();
        ProjectID = reader.ReadGuid() ?? Guid.Empty;
        AssetID = reader.ReadGuid() ?? Guid.Empty;
        IsJohnDoe = reader.ReadBoolean();
      }
      
      if (messageVersion >= 2)
      {
        SubmissionFlags = (TAGFileSubmissionFlags)reader.ReadInt();
      }
      else
      {
        SubmissionFlags = TAGFileSubmissionFlags.AddToArchive;
      }

      if (messageVersion >= 3)
      {
        OriginSource = (TAGFileOriginSource)reader.ReadInt();
      }
      else
      {
        OriginSource = TAGFileOriginSource.LegacyTAGFileSource;
      }
    }
  }
}
