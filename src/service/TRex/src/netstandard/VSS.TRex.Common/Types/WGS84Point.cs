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
      return otherPoint.Lat == Lat && otherPoint.Lon == Lon && otherPoint.Height == Height;
    }

    public override int GetHashCode() => 0;
  }
}
