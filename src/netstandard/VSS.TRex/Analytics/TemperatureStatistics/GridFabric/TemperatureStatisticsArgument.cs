using System;
using VSS.TRex.GridFabric.Models.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.TemperatureStatistics.GridFabric
{
	/// <summary>
	/// Argument containing the parameters required for a Temperature statistics request
	/// </summary>    
	[Serializable]
	public class TemperatureStatisticsArgument : BaseApplicationServiceRequestArgument
  {
		// TODO If desired: ExternalDescriptor :TASNodeRequestDescriptor, which should be moved to the base class;

		/// <summary>
		/// The flag is to indicate wehther or not the temperature warning levels to be user overrides.
		/// </summary>
	  public bool OverrideTemperatureWarningLevels { get; set; }

    /// <summary>
    /// User overriding temperature warning level values.
    /// </summary>
    public TemperatureWarningLevelsRecord OverridingTemperatureWarningLevels;
  }
}
