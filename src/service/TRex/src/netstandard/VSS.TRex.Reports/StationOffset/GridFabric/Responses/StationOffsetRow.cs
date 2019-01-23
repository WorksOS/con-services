using System;
using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Reports.Gridded;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Responses
{
  // todoJeannie move GriddedReportDataRow to a common path?
  public class StationOffsetRow : GriddedReportDataRow, IEquatable<StationOffsetRow>
  {
    private double Offset { get; set; }

    // station is obsolete (as its in derived class StationOffsetReportDataRow),
    //   however it's required for reading in raptor-common 3dp code
    private double Station { get; set; } 

    public StationOffsetRow()
    {
    }

    public StationOffsetRow
    (double offset, double station, double northing, double easting, double elevation,
      double cutFill, short cmv, short mdp, short passCount, short temperature)
    {
      Offset = offset;
      Station = station;
      base.SetValues(northing, easting, elevation, cutFill, cmv, mdp, passCount, temperature);
    }

    public new void Write(BinaryWriter writer)
    {
      base.Write(writer);
      writer.Write(Offset);
      writer.Write(Station);
    }

    public new void Read(BinaryReader reader)
    {
      base.Read(reader);
      Offset = reader.ReadDouble();
      Station = reader.ReadDouble();
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public new void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
      writer.WriteDouble(Offset);
      writer.WriteDouble(Station);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public new void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
      Offset = reader.ReadDouble();
      Station = reader.ReadDouble();
    }

    public bool Equals(StationOffsetRow other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Offset.Equals(other.Offset);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((StationOffsetRow)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ Offset.GetHashCode();
        hashCode = (hashCode * 397) ^ Station.GetHashCode();
        return hashCode;
      }
    }
  }
}
