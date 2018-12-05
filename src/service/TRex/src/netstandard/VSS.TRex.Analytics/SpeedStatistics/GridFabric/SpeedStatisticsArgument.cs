using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.SpeedStatistics.GridFabric
{
	/// <summary>
	/// Argument containing the parameters required for a Speed statistics request
	/// </summary>    
  public class SpeedStatisticsArgument : BaseApplicationServiceRequestArgument
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
  }
}
