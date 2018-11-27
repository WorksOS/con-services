using System;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a Pass Count statistics request
  /// </summary>    
  public class PassCountStatisticsArgument : BaseApplicationServiceRequestArgument, IEquatable<PassCountStatisticsArgument>
  {
    /// <summary>
    /// The flag is to indicate wehther or not the machine Pass Count target range to be user overrides.
    /// </summary>
    public bool OverrideTargetPassCount { get; set; }

    /// <summary>
    /// Pass Count target range.
    /// </summary>
    public PassCountRangeRecord OverridingTargetPassCountRange;

    /// <summary>
    /// Pass Count details values.
    /// </summary>
    public int[] PassCountDetailValues { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteBoolean(OverrideTargetPassCount);

      OverridingTargetPassCountRange.ToBinary(writer);

      writer.WriteIntArray(PassCountDetailValues);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      OverrideTargetPassCount = reader.ReadBoolean();

      OverridingTargetPassCountRange.FromBinary(reader);

      PassCountDetailValues = reader.ReadIntArray();
    }

    public bool Equals(PassCountStatisticsArgument other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return base.Equals(other) && 
             OverridingTargetPassCountRange.Equals(other.OverridingTargetPassCountRange) && 
             OverrideTargetPassCount == other.OverrideTargetPassCount &&

             (Equals(PassCountDetailValues, other.PassCountDetailValues) ||
              (PassCountDetailValues != null && other.PassCountDetailValues != null && PassCountDetailValues.Length == other.PassCountDetailValues.Length && PassCountDetailValues.SequenceEqual(other.PassCountDetailValues)));
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((PassCountStatisticsArgument) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ OverridingTargetPassCountRange.GetHashCode();
        hashCode = (hashCode * 397) ^ OverrideTargetPassCount.GetHashCode();
        hashCode = (hashCode * 397) ^ (PassCountDetailValues != null ? PassCountDetailValues.GetHashCode() : 0);
        return hashCode;
      }
    }
  }
}
