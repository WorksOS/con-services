using System;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a CMV statistics request
  /// </summary>    
  public class CMVStatisticsArgument : BaseApplicationServiceRequestArgument, IEquatable<CMVStatisticsArgument>
  {
    /// <summary>
    /// The flag is to indicate wehther or not the machine CMV target to be user overrides.
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

      OverrideMachineCMV = reader.ReadBoolean();
      OverridingMachineCMV = reader.ReadShort();

      CMVPercentageRange.FromBinary(reader);

      CMVDetailValues = reader.ReadIntArray();
    }

    public bool Equals(CMVStatisticsArgument other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;

      return base.Equals(other) && 
             CMVPercentageRange.Equals(other.CMVPercentageRange) && 
             OverrideMachineCMV == other.OverrideMachineCMV && 
             OverridingMachineCMV == other.OverridingMachineCMV &&
             (Equals(CMVDetailValues, other.CMVDetailValues) ||
              CMVDetailValues != null && other.CMVDetailValues != null &&
              CMVDetailValues.Length == other.CMVDetailValues.Length &&
              CMVDetailValues.SequenceEqual(other.CMVDetailValues));
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((CMVStatisticsArgument) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ CMVPercentageRange.GetHashCode();
        hashCode = (hashCode * 397) ^ OverrideMachineCMV.GetHashCode();
        hashCode = (hashCode * 397) ^ OverridingMachineCMV.GetHashCode();
        hashCode = (hashCode * 397) ^ (CMVDetailValues != null ? CMVDetailValues.GetHashCode() : 0);
        return hashCode;
      }
    }
  }
}
