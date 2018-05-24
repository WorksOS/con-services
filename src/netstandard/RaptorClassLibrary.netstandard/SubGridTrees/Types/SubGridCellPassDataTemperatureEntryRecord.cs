using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Types
{
	/// <summary>
	/// Contains measured temperature value as well as minimum and maximum temperature warning level values.
	/// </summary>
	public struct SubGridCellPassDataTemperatureEntryRecord
  {
	  /// <summary>
	  /// Minimum machine speed value.
	  /// </summary>
	  public ushort MeasuredTemperature { get; set; }

    /// <summary>
    /// Maximum machine speed value.
    /// </summary>
    public TemperatureWarningLevelsRecord TemperatureLevels;

		/// <summary>
		/// /// Constractor with arguments.
		/// </summary>
		/// <param name="measuredTemperature"></param>
		/// <param name="temperatureLevels"></param>
		public SubGridCellPassDataTemperatureEntryRecord(ushort measuredTemperature, TemperatureWarningLevelsRecord temperatureLevels)
	  {
		  MeasuredTemperature = measuredTemperature;
		  TemperatureLevels = temperatureLevels;
	  }

		/// <summary>
		/// Initialises the Min and Max properties with null values.
		/// </summary>
		public void Clear()
	  {
		  MeasuredTemperature = CellPass.NullMaterialTemperatureValue;
			TemperatureLevels.Clear();
	  }
	}
}
