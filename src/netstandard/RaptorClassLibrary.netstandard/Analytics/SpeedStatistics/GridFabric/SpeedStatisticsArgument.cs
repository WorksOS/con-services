using System;
using VSS.TRex.GridFabric.Arguments;

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
		/// Maximum machine speed target value.
		/// </summary>
		public ushort TargetMaxMachineSpeed { get; set; }
		/// <summary>
		/// Minimum machine speed target value.
		/// </summary>
		public ushort TargetMinMachineSpeed { get; set; }
	}
}
