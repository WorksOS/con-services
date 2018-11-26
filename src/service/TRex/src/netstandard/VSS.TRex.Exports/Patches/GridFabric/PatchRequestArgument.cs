using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Exports.Patches.GridFabric
{
  /// <summary>
  /// The argument to be supplied to the Patches request
  /// </summary>
  public class PatchRequestArgument : BaseApplicationServiceRequestArgument, IEquatable<PatchRequestArgument>
  {
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

      Mode = (DisplayMode)reader.ReadInt();
      DataPatchNumber = reader.ReadInt();
      DataPatchSize = reader.ReadInt();
    }

    public bool Equals(PatchRequestArgument other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return base.Equals(other) && 
             Mode == other.Mode && 
             DataPatchNumber == other.DataPatchNumber && 
             DataPatchSize == other.DataPatchSize;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((PatchRequestArgument) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ (int) Mode;
        hashCode = (hashCode * 397) ^ DataPatchNumber;
        hashCode = (hashCode * 397) ^ DataPatchSize;
        return hashCode;
      }
    }
  }
}
