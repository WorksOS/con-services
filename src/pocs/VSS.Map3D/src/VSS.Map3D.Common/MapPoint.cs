/// <summary>
/// Map3D MapPoint
/// </summary>
namespace VSS.Map3D.Common
{
  /// <summary>
  /// WGS84 point on map
  /// </summary>
  public struct MapPoint
  {
    public double Longitude { get; set; } // In Degrees
    public double Latitude { get; set; } // In Degrees

    public MapPoint(double lon, double lat)
    {
      Longitude = lon;
      Latitude = lat;
    }
  }
}
