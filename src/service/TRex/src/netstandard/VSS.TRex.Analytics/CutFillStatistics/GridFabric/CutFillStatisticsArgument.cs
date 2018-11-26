using System;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Analytics.CutFillStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a Cut/Fill statistics request
  /// </summary>    
  public class CutFillStatisticsArgument : BaseApplicationServiceRequestArgument, IEquatable<CutFillStatisticsArgument>
  {
    /// <summary>
    /// The set of cut/fill offsets
    /// Current this is always 7 elements in array and assumes grade is set at zero
    /// eg: 0.5, 0.2, 0.1, 0.0, -0.1, -0.2, -0.5
    /// </summary>
    public double[] Offsets { get; set; }

    /// <summary>
    /// The ID of the design to compute cut fill values between it and the production data elevatoins
    /// </summary>
    public Guid DesignID { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteDoubleArray(Offsets);
      writer.WriteGuid(DesignID);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      Offsets = reader.ReadDoubleArray();
      DesignID = reader.ReadGuid() ?? Guid.Empty;
    }

    public bool Equals(CutFillStatisticsArgument other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return base.Equals(other) &&
             DesignID.Equals(other.DesignID) &&
             (Equals(Offsets, other.Offsets) ||
              Offsets != null && other.Offsets != null &&
              Offsets.Length == other.Offsets.Length &&
              Offsets.SequenceEqual(other.Offsets));
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((CutFillStatisticsArgument) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ (Offsets != null ? Offsets.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ DesignID.GetHashCode();
        return hashCode;
      }
    }
  }
}
