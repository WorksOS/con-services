using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.GridFabric.Responses
{
  public class ProcessTAGFileResponseItem : IProcessTAGFileResponseItem
  {
    private const byte VERSION_NUMBER = 2;
    private static byte[] VERSION_NUMBERS = { 1, 2 };

    public string FileName { get; set; }

    public Guid AssetUid { get; set; }

    public bool Success { get; set; }

    public string Exception { get; set; }

    public TAGReadResult ReadResult { get; set; }

    public TAGFileSubmissionFlags SubmissionFlags { get; set; } = TAGFileSubmissionFlags.AddToArchive;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public ProcessTAGFileResponseItem()
    {
    }

    /// <summary>
    /// Creates a new item and serialises its content from the supplied IBinaryRawReader
    /// </summary>
    public ProcessTAGFileResponseItem(IBinaryRawReader reader)
    {
      FromBinary(reader);
    }

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteString(FileName);
      writer.WriteGuid(AssetUid);
      writer.WriteBoolean(Success);
      writer.WriteString(Exception);
      writer.WriteInt((int)ReadResult);
      writer.WriteInt((int)SubmissionFlags);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      var messageVersion = VersionSerializationHelper.CheckVersionsByte(reader, VERSION_NUMBERS);

      FileName = reader.ReadString();
      AssetUid = reader.ReadGuid() ?? Guid.Empty;
      Success = reader.ReadBoolean();
      Exception = reader.ReadString();
      ReadResult = (TAGReadResult)reader.ReadInt();

      if (messageVersion >= 2)
      {
        SubmissionFlags = (TAGFileSubmissionFlags)reader.ReadInt();
      }
      else
      {
        SubmissionFlags = TAGFileSubmissionFlags.AddToArchive;
      }
    }
  }
}

