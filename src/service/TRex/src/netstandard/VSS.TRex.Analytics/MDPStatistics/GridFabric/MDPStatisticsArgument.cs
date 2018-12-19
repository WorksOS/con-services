using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.MDPStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a MDP statistics request
  /// </summary>    
  public class MDPStatisticsArgument : BaseApplicationServiceRequestArgument
  {
    /// <summary>
    /// The flag is to indicate wehther or not the machine MDP target to be user overrides.
    /// </summary>
    public bool OverrideMachineMDP { get; set; }

    /// <summary>
    /// User overriding MDP target value.
    /// </summary>
    public short OverridingMachineMDP { get; set; }

    /// <summary>
    /// MDP percentage range.
    /// </summary>
    public MDPRangePercentageRecord MDPPercentageRange;

    /// <summary>
    /// MDP details values.
    /// </summary>
    public int[] MDPDetailValues { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteBoolean(OverrideMachineMDP);
      writer.WriteShort(OverridingMachineMDP);

      MDPPercentageRange.ToBinary(writer);

      writer.WriteIntArray(MDPDetailValues);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      OverrideMachineMDP = reader.ReadBoolean();
      OverridingMachineMDP = reader.ReadShort();

      MDPPercentageRange.FromBinary(reader);

      MDPDetailValues = reader.ReadIntArray();
    }
  }
}
