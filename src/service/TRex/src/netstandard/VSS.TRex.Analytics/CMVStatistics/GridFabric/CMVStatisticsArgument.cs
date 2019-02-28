using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a CMV statistics request
  /// </summary>    
  public class CMVStatisticsArgument : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The flag is to indicate whether or not the machine CMV target to be user overrides.
    /// </summary>
    public bool OverrideMachineCMV { get; set; }

    /// <summary>
    /// User overriding CMV target value.
    /// </summary>
    public short OverridingMachineCMV { get; set; }

    /// <summary>
    /// CMV percentage range.
    /// </summary>
   public CMVRangePercentageRecord CMVPercentageRange;

    /// <summary>
    /// CMV details values.
    /// </summary>
    public int[] CMVDetailValues { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(OverrideMachineCMV);
      writer.WriteShort(OverridingMachineCMV);

      CMVPercentageRange.ToBinary(writer);

      writer.WriteIntArray(CMVDetailValues);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      OverrideMachineCMV = reader.ReadBoolean();
      OverridingMachineCMV = reader.ReadShort();

      CMVPercentageRange.FromBinary(reader);

      CMVDetailValues = reader.ReadIntArray();
    }
  }
}
