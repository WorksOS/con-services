using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Exports.Patches.GridFabric
{
  /// <summary>
  /// The argument to be supplied to the Patches request
  /// </summary>
  public class PatchRequestArgument : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The type of data requested for the patch. Single attribute only, expressed as the
    /// user-space display mode of the data
    /// </summary>
    public DisplayMode Mode { get; set; }

    /// <summary>
    /// The number of the patch of sub grids being requested within the overall set of patches that comprise the request
    /// </summary>
    public int DataPatchNumber { get; set; }

    /// <summary>
    /// The maximum number of sub grids to be returned in each patch of sub grids
    /// </summary>
    public int DataPatchSize { get; set; }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt((int)Mode);
      writer.WriteInt(DataPatchNumber);
      writer.WriteInt(DataPatchSize);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        Mode = (DisplayMode) reader.ReadInt();
        DataPatchNumber = reader.ReadInt();
        DataPatchSize = reader.ReadInt();
      }
    }
  }
}
