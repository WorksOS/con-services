using System;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.MDPStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a MDP statistics request
  /// </summary>    
  public class MDPStatisticsArgument : BaseApplicationServiceRequestArgument, IEquatable<MDPStatisticsArgument>
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

    public bool Equals(MDPStatisticsArgument other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return base.Equals(other) && 
             MDPPercentageRange.Equals(other.MDPPercentageRange) && 
             OverrideMachineMDP == other.OverrideMachineMDP && 
             OverridingMachineMDP == other.OverridingMachineMDP &&
             
             (Equals(MDPDetailValues, other.MDPDetailValues) ||
              (MDPDetailValues != null && other.MDPDetailValues != null && MDPDetailValues.Length == other.MDPDetailValues.Length && MDPDetailValues.SequenceEqual(other.MDPDetailValues)));
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((MDPStatisticsArgument) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ MDPPercentageRange.GetHashCode();
        hashCode = (hashCode * 397) ^ OverrideMachineMDP.GetHashCode();
        hashCode = (hashCode * 397) ^ OverridingMachineMDP.GetHashCode();
        hashCode = (hashCode * 397) ^ (MDPDetailValues != null ? MDPDetailValues.GetHashCode() : 0);
        return hashCode;
      }
    }
  }
}
