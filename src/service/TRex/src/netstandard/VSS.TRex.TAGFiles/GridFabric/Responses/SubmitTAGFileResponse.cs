﻿using System.Diagnostics;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.GridFabric.Responses
{
  /// <summary>
  /// Code if negative means it was generated by mutable SubmitTAGFileExecutor checks. If positive it means its the code from TFA service validation
  /// </summary>
  public class SubmitTAGFileResponse : BaseRequestResponse
  {
    public const byte versionNumber = 1;

    public string FileName { get; set; }

    public bool Success { get; set; }

    public int Code { get; set; }

    public string Message { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public SubmitTAGFileResponse()
    {
    }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(versionNumber);
      writer.WriteString(FileName);
      writer.WriteBoolean(Success);
      writer.WriteInt(Code);
      writer.WriteString(Message);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      byte readVersionNumber = reader.ReadByte();

      Debug.Assert(readVersionNumber == versionNumber, $"Invalid version number: {readVersionNumber}, expecting {versionNumber}");

      FileName = reader.ReadString();
      Success = reader.ReadBoolean();
      Code = reader.ReadInt();
      Message = reader.ReadString();
    }
  }
}
