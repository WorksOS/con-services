using Apache.Ignite.Core.Binary;

namespace VSS.TRex.Common.Models
{
  /// <summary>
  /// The argument to be supplied to the grid request
  /// </summary>
  public class StationOffsetPoint 
  {
    public double Station { get; set; }

    public double Offset { get; set; }

    /// <summary>
    /// Offsets left and right (or on) the center line in the AlignmentUid design
    /// </summary>
    /// 
    public double Northing { get; set; }

    public double Easting { get; set; }

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
  }
}
