using System;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.TemperatureStatistics.GridFabric
{
	/// <summary>
	/// Argument containing the parameters required for a Temperature statistics request
	/// </summary>    
	public class TemperatureStatisticsArgument : BaseApplicationServiceRequestArgument, IEquatable<BaseApplicationServiceRequestArgument>
  {
		/// <summary>
		/// The flag is to indicate whether or not the temperature warning levels to be user overrides.
		/// </summary>
	  public bool OverrideTemperatureWarningLevels { get; set; }

    /// <summary>
    /// User overriding temperature warning level values.
    /// </summary>
    public TemperatureWarningLevelsRecord OverridingTemperatureWarningLevels;

    /// <summary>
    /// Temperature details values.
    /// </summary>
    public int[] TemperatureDetailValues { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteBoolean(OverrideTemperatureWarningLevels);

      OverridingTemperatureWarningLevels.ToBinary(writer);

      writer.WriteIntArray(TemperatureDetailValues);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      OverrideTemperatureWarningLevels = reader.ReadBoolean();

      OverridingTemperatureWarningLevels.FromBinary(reader);

      TemperatureDetailValues = reader.ReadIntArray();
    }

    protected bool Equals(TemperatureStatisticsArgument other)
    {
      return base.Equals(other) && 
             OverridingTemperatureWarningLevels.Equals(other.OverridingTemperatureWarningLevels) && 
             OverrideTemperatureWarningLevels == other.OverrideTemperatureWarningLevels && 
             (Equals(TemperatureDetailValues, other.TemperatureDetailValues) ||
             (TemperatureDetailValues != null && other.TemperatureDetailValues != null && TemperatureDetailValues.SequenceEqual(other.TemperatureDetailValues)));
    }

    public bool Equals(BaseApplicationServiceRequestArgument other)
    {
      return Equals(other as TemperatureStatisticsArgument);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((TemperatureStatisticsArgument) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ OverridingTemperatureWarningLevels.GetHashCode();
        hashCode = (hashCode * 397) ^ OverrideTemperatureWarningLevels.GetHashCode();
        hashCode = (hashCode * 397) ^ (TemperatureDetailValues != null ? TemperatureDetailValues.GetHashCode() : 0);
        return hashCode;
      }
    }
  }
}
