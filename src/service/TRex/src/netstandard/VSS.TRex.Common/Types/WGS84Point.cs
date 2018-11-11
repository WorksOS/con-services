using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

namespace VSS.TRex.Types
{
  /// <summary>
  /// A two dimensional point defining Lattitude and Longitude in the WGS84 datum
  /// </summary>
  public class WGS84Point
  {
    // Note: Lat and Lon expressed as radians, Height as meters
    public double Lat;
    public double Lon;
    public double Height = Consts.NullDouble;

    public WGS84Point(double lon, double lat) { Lat = lat; Lon = lon; }
    public WGS84Point(double lon, double lat, double height) { Lat = lat; Lon = lon; Height = height; }

    public override bool Equals(object obj)
    {
      var otherPoint = obj as WGS84Point;
      if (otherPoint == null) return false;
      return Math.Abs(otherPoint.Lat - Lat) < Consts.TOLERANCE_DECIMAL_DEGREE && 
             Math.Abs(otherPoint.Lon - Lon) < Consts.TOLERANCE_DECIMAL_DEGREE && 
             Math.Abs(otherPoint.Height - Height) < Consts.TOLERANCE_DIMENSION;
    }

    public override int GetHashCode() => 0;

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteDouble(Lat);
      writer.WriteDouble(Lon);
      writer.WriteDouble(Height);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      Lat = reader.ReadDouble();
      Lon = reader.ReadDouble();
      Height = reader.ReadDouble();
    }
  }
}
