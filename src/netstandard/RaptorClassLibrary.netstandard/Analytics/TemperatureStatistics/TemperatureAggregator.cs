using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Analytics.Aggregators;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;
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
		/// Holds last known good minimum temperature level value.
		/// </summary>
		public ushort LastTempRangeMin { get; private set; }

		/// <summary>
		/// Holds last known good maximum temperature level value.
		/// </summary>
		public ushort LastTempRangeMax { get; private set; }

		/// <summary>
		/// Default no-arg constructor
		/// </summary>
		public TemperatureAggregator()
		{
			// ...
		}

		protected override void DataCheck(AggregatorBase other)
		{
			var aggregator = (TemperatureAggregator) other;

			if (IsTargetValueConstant && other.SummaryCellsScanned > 0) // if we need to check for a difference
			{
				// compare grouped results to determine if target varies
				if (aggregator.LastTempRangeMax != CellPass.NullMaterialTemperatureValue && LastTempRangeMax != CellPass.NullMaterialTemperatureValue) // If the data is valid...
				{
					if (LastTempRangeMax != aggregator.LastTempRangeMax) // Compare...
						IsTargetValueConstant = false;
				}

				if (aggregator.LastTempRangeMin != CellPass.NullMaterialTemperatureValue && LastTempRangeMin != CellPass.NullMaterialTemperatureValue) // If the data is valid...
				{
					if (LastTempRangeMin != aggregator.LastTempRangeMin) // Compare...
						IsTargetValueConstant = false;
				}
			};

			if (aggregator.LastTempRangeMax != CellPass.NullMaterialTemperatureValue) // If the data is valid...
				LastTempRangeMax = aggregator.LastTempRangeMax;  // Set the value...

			if (aggregator.LastTempRangeMin != CellPass.NullMaterialTemperatureValue) // If the data is valid...
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

			var currentTempRangeMax = CellPass.NullMaterialTemperatureValue;
			var currentTempRangeMin = CellPass.NullMaterialTemperatureValue;

			SubGridUtilities.SubGridDimensionalIterator((I, J) =>
			{
				var temperatureValue = SubGrid.Cells[I, J];
				if (temperatureValue.MeasuredTemperature != CellPass.NullMaterialTemperatureValue) // is there a value to test
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
						if (IsTargetValueConstant) // Do we need to test...
						{
							if (temperatureValue.TemperatureLevels.Min != CellPass.NullMaterialTemperatureValue && LastTempRangeMin != CellPass.NullMaterialTemperatureValue) // Values all good to test...
								IsTargetValueConstant = LastTempRangeMin == temperatureValue.TemperatureLevels.Min; // Check to see if the target value varies...
						}

						if (LastTempRangeMin != temperatureValue.TemperatureLevels.Min && temperatureValue.TemperatureLevels.Min != CellPass.NullMaterialTemperatureValue)
							LastTempRangeMin = temperatureValue.TemperatureLevels.Min; // ConstantTempRangeMin holds last good value

						if (currentTempRangeMin != temperatureValue.TemperatureLevels.Min)
						 currentTempRangeMin = temperatureValue.TemperatureLevels.Min;

						// Maximum level value...
						if (IsTargetValueConstant) // Do we need to test...
						{
							if (temperatureValue.TemperatureLevels.Max != CellPass.NullMaterialTemperatureValue && LastTempRangeMax != CellPass.NullMaterialTemperatureValue) // Values all good to test...
								IsTargetValueConstant = LastTempRangeMax == temperatureValue.TemperatureLevels.Max; // Check to see if the target value varies...
						}

						if (LastTempRangeMax != temperatureValue.TemperatureLevels.Max && temperatureValue.TemperatureLevels.Max != CellPass.NullMaterialTemperatureValue)
							LastTempRangeMax = temperatureValue.TemperatureLevels.Max; // ConstantTempRangeMax holds last good value

						if (currentTempRangeMax != temperatureValue.TemperatureLevels.Max)
							currentTempRangeMax = temperatureValue.TemperatureLevels.Max;
					}

					// Is the range good?..
					if (currentTempRangeMin != CellPass.NullMaterialTemperatureValue && currentTempRangeMax != CellPass.NullMaterialTemperatureValue)
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
					else // we have data but no target data to do a summary
						MissingTargetValue = true; // flag to issue a warning to user
				}
			});
		}
	}
}
