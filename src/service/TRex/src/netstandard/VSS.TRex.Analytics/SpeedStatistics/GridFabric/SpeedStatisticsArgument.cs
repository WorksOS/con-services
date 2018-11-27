using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.SpeedStatistics.GridFabric
{
	/// <summary>
	/// Argument containing the parameters required for a Speed statistics request
	/// </summary>    
  public class SpeedStatisticsArgument : BaseApplicationServiceRequestArgument, IEquatable<SpeedStatisticsArgument>
  {
	  /// <summary>
	  /// Machine speed target record. It contains min/max machine speed target value.
	  /// </summary>
	  public MachineSpeedExtendedRecord TargetMachineSpeed;

	  /// <summary>
	  /// Serialises content to the writer
	  /// </summary>
	  /// <param name="writer"></param>
	  public override void ToBinary(IBinaryRawWriter writer)
	  {
	    base.ToBinary(writer);

	    TargetMachineSpeed.ToBinary(writer);
	  }

	  /// <summary>
	  /// Serialises content from the writer
	  /// </summary>
	  /// <param name="reader"></param>
	  public override void FromBinary(IBinaryRawReader reader)
	  {
	    base.FromBinary(reader);

	    TargetMachineSpeed.FromBinary(reader);
	  }

    public bool Equals(SpeedStatisticsArgument other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return base.Equals(other) && 
             TargetMachineSpeed.Equals(other.TargetMachineSpeed);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((SpeedStatisticsArgument) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (base.GetHashCode() * 397) ^ TargetMachineSpeed.GetHashCode();
      }
    }
  }
}
