using System.IO;
using VSS.TRex.Cells;

namespace VSS.TRex.SubGridTrees.Types
{
  /// <summary>
  /// Contains measured and target Pass Count values as well as previous measured and target CMV values.
  /// </summary>
  public struct SubGridCellPassDataPassCountEntryRecord
  {
    /// <summary>
    /// Measured Pass Count value.
    /// </summary>
    public ushort MeasuredPassCount { get; set; }

    /// <summary>
    /// Target Pass Count value.
    /// </summary>
    public ushort TargetPassCount { get; set; }

    /// <summary>
    ///  Constractor with arguments.
    /// </summary>
    /// <param name="measuredPassCount"></param>
    /// <param name="targetPassCount"></param>
    public SubGridCellPassDataPassCountEntryRecord(ushort measuredPassCount, ushort targetPassCount)
    {
      MeasuredPassCount = measuredPassCount;
      TargetPassCount = targetPassCount;
    }

    /// <summary>
    /// Initialises the measured and target Pass Count properties with null values.
    /// </summary>
    public void Clear()
    {
      MeasuredPassCount = CellPass.NullPassCountValue;
      TargetPassCount = CellPass.NullPassCountValue;
    }

    /// <summary>
    /// Serialises content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(MeasuredPassCount);
      writer.Write(TargetPassCount);
    }

    /// <summary>
    /// Serialises comtent of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      MeasuredPassCount = reader.ReadUInt16();
      TargetPassCount = reader.ReadUInt16();
    }

    /// <summary>
    /// Defines a publically accessible null value for this cell value type
    /// </summary>
    public static SubGridCellPassDataPassCountEntryRecord NullValue = SubGridCellPassDataPassCountEntryRecord.Null();

    /// <summary>
    /// Implements the business logic to create the null value for this cell valuye type
    /// </summary>
    /// <returns></returns>
    private static SubGridCellPassDataPassCountEntryRecord Null()
    {
      SubGridCellPassDataPassCountEntryRecord result = new SubGridCellPassDataPassCountEntryRecord();
      result.Clear();
      return result;
    }

    public bool Equals(SubGridCellPassDataPassCountEntryRecord other)
    {
      return MeasuredPassCount == other.MeasuredPassCount &&
             TargetPassCount == other.TargetPassCount;
    }
  }
}
