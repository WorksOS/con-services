using System;
using System.IO;
using VSS.TRex.Common;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// The content of each cell in a 'Height and time' client leaf sub grid. Each cell stores an elevation and 
  /// the time at which the elevation measurement relates to (either the time stamp on a cell pass or the time
  /// stamp from the surveyed surface that this elevation came from
  /// Note: Do not implement any interfaces on this struct
  /// </summary>
  public struct SubGridCellHeightAndTime
  {
    /// <summary>
    /// Measure height at the cell location
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// UTC time at which the measurement is relevant
    /// </summary>
    public long Time { get; set; }

    /// <summary>
    /// Set Height and Time values to null
    /// </summary>
    public void Clear()
    {
      Time = DateTime.MinValue.Ticks;
      Height = Consts.NullHeight;
    }

    /// <summary>
    /// Sets height and time components of the struct in a single operation
    /// </summary>
    /// <param name="height"></param>
    /// <param name="time"></param>
    public void Set(float height, long time)
    {
      Height = height;
      Time = time;
    }

    /// <summary>
    /// Determine if this height and time is the same as 'other'
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(SubGridCellHeightAndTime other)
    {
      return Height == other.Height && Time == other.Time;
    }

    /// <summary>
    /// Serialise out the height and time information
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(Height);
      writer.Write(Time);
    }

    /// <summary>
    /// Serialise out the height and tiem information
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      Height = reader.ReadSingle();
      Time = reader.ReadInt64();
    }
  }
}
