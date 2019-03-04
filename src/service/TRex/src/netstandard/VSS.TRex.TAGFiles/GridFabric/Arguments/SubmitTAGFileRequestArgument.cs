﻿using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.TAGFiles.GridFabric.Arguments
{
  public class SubmitTAGFileRequestArgument : BaseRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// Overridden ID of the project to process the TAG files into
    /// </summary>
    public Guid? ProjectID { get; set; }

    /// <summary>
    /// Overridden ID of the asset to process the TAG files into
    /// </summary>
    //public long AssetUID { get; set; } = -1;
    public Guid? AssetID { get; set; }

    /// <summary>
    /// Name of physical tag file
    /// </summary>
    public string TAGFileName { get; set; } = string.Empty;

    /// <summary>
    /// The content of the TAG file being submitted
    /// </summary>
    public byte[] TagFileContent { get; set; }

    /// <summary>
    /// Helps TFA service determine correct project
    /// </summary>
    public string TCCOrgID { get; set; } = string.Empty;

    /// <summary>
    ///  Default no-arg constructor
    /// </summary>
    public SubmitTAGFileRequestArgument()
    {
    }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectID);
      writer.WriteGuid(AssetID);
      writer.WriteString(TAGFileName);
      writer.WriteString(TCCOrgID);
      writer.WriteByteArray(TagFileContent);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ProjectID = reader.ReadGuid();
      AssetID = reader.ReadGuid();
      TAGFileName = reader.ReadString();
      TCCOrgID = reader.ReadString();
      TagFileContent = reader.ReadByteArray();
    }
  }
}
