using System.IO;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Types
{
	/// <summary>
	/// Contains measured temperature value as well as minimum and maximum temperature warning level values.
	/// </summary>
	public struct SubGridCellPassDataTemperatureEntryRecord
  {
    /// <summary>
    /// Measured temperature value.
    /// </summary>
    public ushort MeasuredTemperature { get; set; }

    /// <summary>
    /// Temperature warning levels.
    /// </summary>
    public TemperatureWarningLevelsRecord TemperatureLevels;

    /// <summary>
    /// Return an indicative size for memory consumption of this class to be used in cache tracking
    /// </summary>
    /// <returns></returns>
    public static int IndicativeSizeInBytes()
    {
      return sizeof(ushort) + 4 * TemperatureWarningLevelsRecord.IndicativeSizeInBytes(); 
    }

    /// <summary>
    /// Constructor with arguments.
    /// </summary>
    /// <param name="measuredTemperature"></param>
    /// <param name="temperatureLevels"></param>
    public SubGridCellPassDataTemperatureEntryRecord(ushort measuredTemperature, TemperatureWarningLevelsRecord temperatureLevels)
	  {
		  MeasuredTemperature = measuredTemperature;
		  TemperatureLevels = temperatureLevels;
	  }

		/// <summary>
		/// Initializes the measured temperature and its warning levels with null values.
		/// </summary>
		public void Clear()
	  {
		  MeasuredTemperature = CellPassConsts.NullMaterialTemperatureValue;
			TemperatureLevels.Clear();
	  }

    /// <summary>
    /// Defines a publicly accessible null value for this cell value type
    /// </summary>
    public static SubGridCellPassDataTemperatureEntryRecord NullValue = Null();

    /// <summary>
    /// Implements the business logic to create the null value for this cell value type
    /// </summary>
    /// <returns></returns>
    public static SubGridCellPassDataTemperatureEntryRecord Null()
    {
      SubGridCellPassDataTemperatureEntryRecord Result = new SubGridCellPassDataTemperatureEntryRecord();
      Result.Clear();
      return Result;
    }

    /// <summary>
    /// Serializes content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(MeasuredTemperature);
      TemperatureLevels.Write(writer);
    }

    /// <summary>
    /// Serializes content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      MeasuredTemperature = reader.ReadUInt16();
      TemperatureLevels.Read(reader);
    }

    public bool Equals(SubGridCellPassDataTemperatureEntryRecord other)
    {
      return MeasuredTemperature == other.MeasuredTemperature && TemperatureLevels.Equals(other.TemperatureLevels);
    }
  }
}
