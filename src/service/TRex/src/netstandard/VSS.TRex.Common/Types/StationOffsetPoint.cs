using System;
using Apache.Ignite.Core.Binary;

namespace VSS.TRex.Common.Types
{
  /// <summary>
  /// The argument to be supplied to the grid request
  /// </summary>
  public class StationOffsetPoint : IEquatable<StationOffsetPoint>
  {
    private double Station { get; set; }

    private double Offset { get; set; }

    /// <summary>
    /// Offsets left and right (or on) the center line in the AlignmentUid design
    /// </summary>
    /// 
    private double Northing { get; set; }

    private double Easting { get; set; }

    public StationOffsetPoint()
    {
    }

    public StationOffsetPoint(double station, double offset, double northing, double easting)
    {
      SetValues(station, offset, northing, easting);
    }

    public void SetValues(double station, double offset, double northing, double easting)
    {
      Station = station;
      Offset = offset;
      Northing = northing;
      Easting = easting;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteDouble(Station);
      writer.WriteDouble(Offset);
      writer.WriteDouble(Northing);
      writer.WriteDouble(Easting);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      Station = reader.ReadDouble();
      Offset = reader.ReadDouble();
      Northing = reader.ReadDouble();
      Easting = reader.ReadDouble();
    }

    public bool Equals(StationOffsetPoint other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return base.Equals(other) &&
             Station.Equals(other.Station) &&
             Offset.Equals(other.Offset) &&
             Northing.Equals(other.Northing) &&
             Easting.Equals(other.Easting);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((StationOffsetPoint)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ Station.GetHashCode();
        hashCode = (hashCode * 397) ^ Offset.GetHashCode();
        hashCode = (hashCode * 397) ^ Northing.GetHashCode();
        hashCode = (hashCode * 397) ^ Easting.GetHashCode();
        return hashCode;
      }
    }
  }
}
