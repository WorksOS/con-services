using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.TemperatureStatistics
{
	/// <summary>
	/// Implements the specific business rules for calculating a Temperature summary
	/// </summary>
	public class TemperatureAggregator : SummaryDataAggregator
	{
		/// <summary>
		/// The flag is to indicate wehther or not the temperature warning levels to be user overrides.
		/// </summary>
		public bool OverrideTemperatureWarningLevels { get; set; }

		/// <summary>
		/// User overriding temperature warning level values.
		/// </summary>
		public TemperatureWarningLevelsRecord OverridingTemperatureWarningLevels;

		/// <summary>
		/// Holds last known good minimum temperature level value.
		/// </summary>
		public ushort LastTempRangeMin { get; private set; } = CellPassConsts.NullMaterialTemperatureValue;

		/// <summary>
		/// Holds last known good maximum temperature level value.
		/// </summary>
		public ushort LastTempRangeMax { get; private set; } = CellPassConsts.NullMaterialTemperatureValue;

		/// <summary>
		/// Default no-arg constructor
		/// </summary>
		public TemperatureAggregator()
		{
			OverridingTemperatureWarningLevels.Clear();
		}

		protected override void DataCheck(DataStatisticsAggregator other)
		{
			var aggregator = (TemperatureAggregator) other;

			if (IsTargetValueConstant && aggregator.SummaryCellsScanned > 0) // if we need to check for a difference
			{
				// Compare grouped results to determine if target varies
				if (aggregator.LastTempRangeMax != CellPassConsts.NullMaterialTemperatureValue && LastTempRangeMax != CellPassConsts.NullMaterialTemperatureValue) // If the data is valid...
				{
					if (LastTempRangeMax != aggregator.LastTempRangeMax) // Compare...
						IsTargetValueConstant = false;
				}

				if (aggregator.LastTempRangeMin != CellPassConsts.NullMaterialTemperatureValue && LastTempRangeMin != CellPassConsts.NullMaterialTemperatureValue) // If the data is valid...
				{
					if (LastTempRangeMin != aggregator.LastTempRangeMin) // Compare...
						IsTargetValueConstant = false;
				}
			};

			if (aggregator.LastTempRangeMax != CellPassConsts.NullMaterialTemperatureValue) // If the data is valid...
				LastTempRangeMax = aggregator.LastTempRangeMax;  // Set the value...

			if (aggregator.LastTempRangeMin != CellPassConsts.NullMaterialTemperatureValue) // If the data is valid...
				LastTempRangeMin = aggregator.LastTempRangeMin;  // set value
		}

		/// <summary>
		/// Processes a Temperature subgrid into a temperature isopach and calculate the counts of cells where the Temperature value
		/// fits into the requested bands, i.e. less than min level, between min and max levels, greater than max level
		/// </summary>
		/// <param name="subGrids"></param>
		public override void ProcessSubgridResult(IClientLeafSubGrid[][] subGrids)
		{
			base.ProcessSubgridResult(subGrids);

			// Works out the percentage each colour on the map represents

			if (!(subGrids[0][0] is ClientTemperatureLeafSubGrid SubGrid))
				return;

			var currentTempRangeMax = CellPassConsts.NullMaterialTemperatureValue;
			var currentTempRangeMin = CellPassConsts.NullMaterialTemperatureValue;

			SubGridUtilities.SubGridDimensionalIterator((I, J) =>
			{
				var temperatureValue = SubGrid.Cells[I, J];
				if (temperatureValue.MeasuredTemperature != CellPassConsts.NullMaterialTemperatureValue) // Is there a value to test?..
				{
					if (OverrideTemperatureWarningLevels)
					{
						if (LastTempRangeMin != OverridingTemperatureWarningLevels.Min)
							LastTempRangeMin = OverridingTemperatureWarningLevels.Min;
						if (LastTempRangeMax != OverridingTemperatureWarningLevels.Max)
							LastTempRangeMax = OverridingTemperatureWarningLevels.Max;
						if (currentTempRangeMin != OverridingTemperatureWarningLevels.Min)
							currentTempRangeMin = OverridingTemperatureWarningLevels.Min;
						if (currentTempRangeMax != OverridingTemperatureWarningLevels.Max)
							currentTempRangeMax = OverridingTemperatureWarningLevels.Max;
					}
					else
					{
						// Using the machine target values test if target varies...
						// Minimum level value...
						if (IsTargetValueConstant) // Do we need to test?..
						{
							if (temperatureValue.TemperatureLevels.Min != CellPassConsts.NullMaterialTemperatureValue && LastTempRangeMin != CellPassConsts.NullMaterialTemperatureValue) // Values all good to test...
								IsTargetValueConstant = LastTempRangeMin == temperatureValue.TemperatureLevels.Min; // Check to see if the target value varies...
						}

						if (LastTempRangeMin != temperatureValue.TemperatureLevels.Min && temperatureValue.TemperatureLevels.Min != CellPassConsts.NullMaterialTemperatureValue)
							LastTempRangeMin = temperatureValue.TemperatureLevels.Min; // ConstantTempRangeMin holds last good value...

						if (currentTempRangeMin != temperatureValue.TemperatureLevels.Min)
						 currentTempRangeMin = temperatureValue.TemperatureLevels.Min;

						// Maximum level value...
						if (IsTargetValueConstant) // Do we need to test?..
						{
							if (temperatureValue.TemperatureLevels.Max != CellPassConsts.NullMaterialTemperatureValue && LastTempRangeMax != CellPassConsts.NullMaterialTemperatureValue) // Values all good to test...
								IsTargetValueConstant = LastTempRangeMax == temperatureValue.TemperatureLevels.Max; // Check to see if the target value varies...
						}

						if (LastTempRangeMax != temperatureValue.TemperatureLevels.Max && temperatureValue.TemperatureLevels.Max != CellPassConsts.NullMaterialTemperatureValue)
							LastTempRangeMax = temperatureValue.TemperatureLevels.Max; // ConstantTempRangeMax holds last good value...

						if (currentTempRangeMax != temperatureValue.TemperatureLevels.Max)
							currentTempRangeMax = temperatureValue.TemperatureLevels.Max;
					}

					// Is the range good?..
					if (currentTempRangeMin != CellPassConsts.NullMaterialTemperatureValue && currentTempRangeMax != CellPassConsts.NullMaterialTemperatureValue)
					{
						SummaryCellsScanned++;
						if (temperatureValue.MeasuredTemperature > LastTempRangeMax)
							CellsScannedOverTarget++;
						else
						{
							if (temperatureValue.MeasuredTemperature < LastTempRangeMin)
								CellsScannedUnderTarget++;
							else
								CellsScannedAtTarget++;
						}
					}
					else // We have data but no target data to do a summary...
						MissingTargetValue = true; // Flag to issue a warning to user...
				}
			});
		}
	}
}
