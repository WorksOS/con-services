using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Analytics.Aggregators;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.TemperatureStatistics
{
	/// <summary>
	/// Implements the specific business rules for calculating a Temperature summary
	/// </summary>
	public class TemperatureAggregator : AggregatorBase
	{
		/// <summary>
		/// The flag is to indicate wehther or not the temperature warning levels to be user overrides.
		/// </summary>
		public bool OverrideTemperatureWarningLevels { get; set; }

		/// <summary>
		/// User overriding temperature warning level values.
		/// </summary>
		public TemperatureWarningLevelsRecord OverridingTemperatureWarningLevels { get; set; }

		/// <summary>
		/// Default no-arg constructor
		/// </summary>
		public TemperatureAggregator()
		{
			// ...
		}

	}
}
