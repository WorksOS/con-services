using System;
using VSS.TRex.GridFabric.Models.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.SpeedStatistics.GridFabric
{
	/// <summary>
	/// Argument containing the parameters required for a Speed statistics request
	/// </summary>    
	[Serializable]
  public class SpeedStatisticsArgument : BaseApplicationServiceRequestArgument
	{
	  /// <summary>
	  /// Machine speed target record. It contains min/max machine speed target value.
	  /// </summary>
	  public MachineSpeedExtendedRecord TargetMachineSpeed;
	}
}
