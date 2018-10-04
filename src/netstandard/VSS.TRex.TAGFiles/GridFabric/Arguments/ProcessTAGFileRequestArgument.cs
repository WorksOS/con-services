using System;
using System.Collections.Generic;
using System.Diagnostics;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.GridFabric.Arguments
{
  public class ProcessTAGFileRequestArgument : BaseRequestBinarizableArgument
  {
    public const byte versionNumber = 1;

    /// <summary>
    /// ID of the project to process the TAG files into
    /// </summary>
    public Guid ProjectID { get; set; } = Guid.Empty;

    /// <summary>
    /// ID of the asset to process the TAG files into
    /// </summary>
    // public long AssetID { get; set; } = -1;
    public Guid AssetID { get; set; }

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
      writer.WriteByte(versionNumber);
      writer.WriteGuid(ProjectID);
      writer.WriteGuid(AssetID);

      writer.WriteInt(TAGFiles.Count);
      foreach (var tagFile in TAGFiles)
        tagFile.ToBinary(writer);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      byte readVersionNumber = reader.ReadByte();

      Debug.Assert(readVersionNumber == versionNumber, $"Invalid version number: {readVersionNumber}, expecting {versionNumber}");

      for (int i = 0; i < reader.ReadInt(); i++)
      {
        TAGFiles.Add(new ProcessTAGFileRequestFileItem(reader));
      }
    }
  }
}
