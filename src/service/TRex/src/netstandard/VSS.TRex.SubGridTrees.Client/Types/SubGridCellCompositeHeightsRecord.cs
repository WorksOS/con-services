using System.IO;
using VSS.TRex.Common;

namespace VSS.TRex.SubGridTrees.Client.Types
{
  /// <summary>
  /// Stores the heights representing the first measure elevation, last measured elevation,
  /// lowest measured elevation and highest measured elevation spanning a set of cell passes
  /// and optionally a set of surveyed surfaces within a time range.
  /// Note: Do not implement any interfaces on this record
  /// </summary>
  public struct SubGridCellCompositeHeightsRecord
  {
    /// <summary>
    /// The four elevations are expressed in meters above the project calibration datum
    /// </summary>
    public float LowestHeight, HighestHeight, LastHeight, FirstHeight;

    /// <summary>
    /// The four dates are expressed as the binary representation of a DateTime to promote efficient
    /// serialization
    /// </summary>
    public long LowestHeightTime, HighestHeightTime, LastHeightTime, FirstHeightTime;

    /// <summary>
    /// Return an indicative size for memory consumption of this class to be used in cache tracking
    /// </summary>
    /// <returns></returns>
    public static int IndicativeSizeInBytes()
    {
      return 4 * sizeof(float) + 4 * sizeof(long); // 4 floats (@four bytes) + four longs (@ 6 bytes)
    }

    public void Clear()
    {
      LowestHeight = Consts.NullHeight;
      HighestHeight = Consts.NullHeight;
      LastHeight = Consts.NullHeight;
      FirstHeight = Consts.NullHeight;
      LowestHeightTime = 0; //DateTime.MinValue;
      HighestHeightTime = 0; //DateTime.MinValue;
      LastHeightTime = 0; //DateTime.MinValue;
      FirstHeightTime = 0; //DateTime.MinValue;
    }

    /// <summary>
    /// Defines a publicly accessible null value for this cell value type
    /// </summary>
    public static SubGridCellCompositeHeightsRecord NullValue = Null();
    
    /// <summary>
    /// Implements the business logic to create the null value for this cell value type
    /// </summary>
    /// <returns></returns>
    public static SubGridCellCompositeHeightsRecord Null()
    {
      SubGridCellCompositeHeightsRecord Result = new SubGridCellCompositeHeightsRecord();
      Result.Clear();
      return Result;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(FirstHeight);
      writer.Write(LastHeight);
      writer.Write(LowestHeight);
      writer.Write(HighestHeight);

      writer.Write(FirstHeightTime);
      writer.Write(LastHeightTime);
      writer.Write(LowestHeightTime);
      writer.Write(HighestHeightTime);
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      FirstHeight = reader.ReadSingle();
      LastHeight = reader.ReadSingle();
      LowestHeight = reader.ReadSingle();
      HighestHeight = reader.ReadSingle();

      FirstHeightTime = reader.ReadInt64();
      LastHeightTime = reader.ReadInt64();
      LowestHeightTime = reader.ReadInt64();
      HighestHeightTime = reader.ReadInt64();
    }

    public bool Equals(SubGridCellCompositeHeightsRecord other)
    {
      return FirstHeight == other.FirstHeight &&
        LastHeight == other.LastHeight &&
        LowestHeight == other.LowestHeight &&
        HighestHeight == other.HighestHeight &&
        FirstHeightTime == other.FirstHeightTime &&
        LastHeightTime == other.LastHeightTime &&
        LowestHeightTime == other.LowestHeightTime &&
        HighestHeightTime == other.HighestHeightTime;
    }

    /// <summary>
    /// Sets all the elevations in the cell to zero
    /// </summary>
    public void SetToZeroHeight()
    {
      LowestHeight = 0;
      HighestHeight = 0;
      LastHeight = 0;
      FirstHeight = 0;
    }
  }
}
