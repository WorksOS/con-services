using System;
using System.IO;
using VSS.TRex.Common.CellPasses;

namespace VSS.TRex.SubGridTrees.Client.Types
{
  /// <summary>
  /// Contains measured and target Pass Count values as well as previous measured and target CMV values.
  /// </summary>
  public struct SubGridCellPassDataPassCountEntryRecord : IEquatable<SubGridCellPassDataPassCountEntryRecord>
  {
    private const byte USHORT_TYPES_COUNT = 2;

    /// <summary>
    /// Measured Pass Count value.
    /// </summary>
    public ushort MeasuredPassCount { get; set; }

    /// <summary>
    /// Target Pass Count value.
    /// </summary>
    public ushort TargetPassCount { get; set; }

    /// <summary>
    /// Return an indicative size for memory consumption of this class to be used in cache tracking
    /// </summary>
    /// <returns></returns>
    public static int IndicativeSizeInBytes() => USHORT_TYPES_COUNT * sizeof(ushort);

      /// <summary>
      ///  Constructor with arguments.
      /// </summary>
      /// <param name="measuredPassCount"></param>
      /// <param name="targetPassCount"></param>
      public SubGridCellPassDataPassCountEntryRecord(ushort measuredPassCount, ushort targetPassCount)
    {
      MeasuredPassCount = measuredPassCount;
      TargetPassCount = targetPassCount;
    }

    /// <summary>
    /// Initializes the measured and target Pass Count properties with null values.
    /// </summary>
    public void Clear()
    {
      MeasuredPassCount = CellPassConsts.NullPassCountValue;
      TargetPassCount = CellPassConsts.NullPassCountValue;
    }

    /// <summary>
    /// Serializes content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(MeasuredPassCount);
      writer.Write(TargetPassCount);
    }

    /// <summary>
    /// Serializes content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      MeasuredPassCount = reader.ReadUInt16();
      TargetPassCount = reader.ReadUInt16();
    }

    /// <summary>
    /// Defines a publicly accessible null value for this cell value type
    /// </summary>
    public static SubGridCellPassDataPassCountEntryRecord NullValue = Null();

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

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      return obj is SubGridCellPassDataPassCountEntryRecord && Equals((SubGridCellPassDataPassCountEntryRecord) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (MeasuredPassCount.GetHashCode() * 397) ^ TargetPassCount.GetHashCode();
      }
    }
  }
}
