using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.TemperatureStatistics.GridFabric
{
	/// <summary>
	/// Argument containing the parameters required for a Temperature statistics request
	/// </summary>    
	public class TemperatureStatisticsArgument : BaseApplicationServiceRequestArgument
  {
		/// <summary>
		/// The flag is to indicate whether or not the temperature warning levels to be user overrides.
		/// </summary>
	  public bool OverrideTemperatureWarningLevels { get; set; }

    /// <summary>
    /// User overriding temperature warning level values.
    /// </summary>
    public TemperatureWarningLevelsRecord OverridingTemperatureWarningLevels;
  }
}
