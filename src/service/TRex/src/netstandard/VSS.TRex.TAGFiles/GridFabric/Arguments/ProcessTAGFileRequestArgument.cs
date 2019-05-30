using System;
using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.TAGFiles.GridFabric.Arguments
{
  public class ProcessTAGFileRequestArgument : BaseRequestArgument
  {
    public const byte VERSION_NUMBER = 1;

    /// <summary>
    /// ID of the project to process the TAG files into
    /// </summary>
    public Guid ProjectID { get; set; } = Guid.Empty;

    /// <summary>
    /// ID of the asset to process the TAG files into
    /// </summary>
    // public long AssetUID { get; set; } = -1;
    public Guid AssetUID { get; set; }

    /// <summary>
    /// A dictionary mapping TAG file names to the content of each file
    /// </summary>
    public List<ProcessTAGFileRequestFileItem> TAGFiles { get; set; }

    /// <summary>
    ///  Default no-arg constructor
    /// </summary>
    public ProcessTAGFileRequestArgument()
    {
    }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(ProjectID);
      writer.WriteGuid(AssetUID);

      writer.WriteInt(TAGFiles?.Count ?? 0);
      if (TAGFiles != null)
      {
        foreach (var tagFile in TAGFiles)
          tagFile.ToBinary(writer);
      }
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ProjectID = reader.ReadGuid() ?? Guid.Empty;
      AssetUID = reader.ReadGuid() ?? Guid.Empty;

      var numTAGFiles = reader.ReadInt();
      TAGFiles = new List<ProcessTAGFileRequestFileItem>(numTAGFiles);

      for (int i = 0; i < numTAGFiles; i++)
        TAGFiles.Add(new ProcessTAGFileRequestFileItem(reader));
    }
  }
}
