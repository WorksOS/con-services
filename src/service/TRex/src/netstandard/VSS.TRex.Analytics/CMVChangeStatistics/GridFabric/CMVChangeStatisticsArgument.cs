using System;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Analytics.CMVChangeStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a CMV change statistics request.
  /// The CMV change is exposed on the client as CMV % change.
  /// </summary>    
  public class CMVChangeStatisticsArgument : BaseApplicationServiceRequestArgument, IEquatable<CMVChangeStatisticsArgument>
  {
    /// <summary>
    /// CMV change details values.
    /// </summary>
    public double[] CMVChangeDetailsDatalValues { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteDoubleArray(CMVChangeDetailsDatalValues);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      CMVChangeDetailsDatalValues = reader.ReadDoubleArray();
    }

    public bool Equals(CMVChangeStatisticsArgument other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return base.Equals(other) && 
             (Equals(CMVChangeDetailsDatalValues, other.CMVChangeDetailsDatalValues) ||
              CMVChangeDetailsDatalValues != null && other.CMVChangeDetailsDatalValues != null &&
              CMVChangeDetailsDatalValues.Length == other.CMVChangeDetailsDatalValues.Length &&
              CMVChangeDetailsDatalValues.SequenceEqual(other.CMVChangeDetailsDatalValues));
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((CMVChangeStatisticsArgument) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (base.GetHashCode() * 397) ^ (CMVChangeDetailsDatalValues != null ? CMVChangeDetailsDatalValues.GetHashCode() : 0);
      }
    }
  }
}
