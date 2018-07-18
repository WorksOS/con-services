using System;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.SpeedStatistics.GridFabric
{
	/// <summary>
	/// Argument containing the parameters required for a Speed statistics request
	/// </summary>    
	[Serializable]
  public class SpeedStatisticsArgument : BaseApplicationServiceRequestArgument
	{
    // TODO If desired: ExternalDescriptor :TASNodeRequestDescriptor, which should be moved to the base class;

	  /// <summary>
	  /// Machine speed target record. It contains min/max machine speed target value.
	  /// </summary>
	  public MachineSpeedExtendedRecord TargetMachineSpeed;
	}
}
