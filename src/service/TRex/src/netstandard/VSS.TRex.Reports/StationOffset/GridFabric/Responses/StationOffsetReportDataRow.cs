using System;
using System.IO;
using Apache.Ignite.Core.Binary;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Responses
{
  /// <summary>
  /// Contains the prepared result for the client to consume
  /// </summary>
  public class StationOffsetReportDataRow : IEquatable<StationOffsetReportDataRow>
  {
    public double Station { get; set; }

    private StationOffsetRow[] Offsets { get; set; }

    // todoJeannie
    private OffsetStatistics Minimum { get; set; }
    private OffsetStatistics Maximum { get; set; }
    private OffsetStatistics Average { get; set; }

    public StationOffsetReportDataRow()
    {
    }

    public StationOffsetReportDataRow(
      double station, int offsetCount, double elevation,
      double cutFill, short cmv, short mdp, short passCount, short temperature)
    {
      Station = station;
      Offsets = new StationOffsetRow[offsetCount];
      Offsets[0] = new StationOffsetRow(0,0,0,0,0,0,0,0,0,0);
      // todoJeannie actually pass all station values
      Maximum = new OffsetStatistics(OffsetStatisticType.Maximum, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
      Minimum = new OffsetStatistics(OffsetStatisticType.Minimum, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
      Average = new OffsetStatistics(OffsetStatisticType.Average, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(Station);
      writer.Write(Offsets.Length);
      foreach (var offset in Offsets)
      {
        offset.Write(writer);
      }
      Minimum.Write(writer);
      Maximum.Write(writer);
      Average.Write(writer);
    }

    public void Read(BinaryReader reader)
    {
      Station = reader.ReadDouble();
      var offsetCount = reader.ReadInt16();
      Offsets = new StationOffsetRow[offsetCount];
      for (var i = 0; i < offsetCount; i++)
      {
        Offsets[i].Read(reader);
      }
      Minimum.Read(reader);
      Maximum.Read(reader);
      Average.Read(reader);
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteDouble(Station);
      writer.WriteInt(Offsets.Length);
      foreach (var offset in Offsets)
      {
        offset.ToBinary(writer);
      }
      Minimum.ToBinary(writer);
      Maximum.ToBinary(writer);
      Average.ToBinary(writer);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      Station = reader.ReadDouble();
      var offsetCount = reader.ReadInt();
      Offsets = new StationOffsetRow[offsetCount];
      for (var i = 0; i < offsetCount; i++)
      {
        Offsets[i].FromBinary(reader);
      }
      Minimum.FromBinary(reader);
      Maximum.FromBinary(reader);
      Average.FromBinary(reader);
    }

    public bool Equals(StationOffsetReportDataRow other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Station.Equals(other.Station) &&
             Offsets.Equals(other.Offsets) && 
             Minimum.Equals(other.Minimum) &&
             Maximum.Equals(other.Maximum) &&
             Average.Equals(other.Average);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((StationOffsetReportDataRow)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ Station.GetHashCode();
        hashCode = (hashCode * 397) ^ Offsets.GetHashCode();
        hashCode = (hashCode * 397) ^ Minimum.GetHashCode();
        hashCode = (hashCode * 397) ^ Maximum.GetHashCode();
        hashCode = (hashCode * 397) ^ Average.GetHashCode();
        return hashCode;
      }
    }
  }
}
