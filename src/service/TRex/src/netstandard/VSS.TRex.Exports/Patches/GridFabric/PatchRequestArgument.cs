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

    // FReferenceVolumeType : TComputeICVolumesType;

    // FICOptions : TSVOICOptions;

    /// <summary>
    /// The number of the patch of subgrids being requested within the overall set of patches that comprise the request
    /// </summary>
    public int DataPatchNumber { get; set; }

    /// <summary>
    /// The maximum number of subgrids to be returned in each patch of subgrids
    /// </summary>
    public int DataPatchSize { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt((int)Mode);
      writer.WriteInt(DataPatchNumber);
      writer.WriteInt(DataPatchSize);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      Mode = (DisplayMode)reader.ReadInt();
      DataPatchNumber = reader.ReadInt();
      DataPatchSize = reader.ReadInt();
    }
  }
}
