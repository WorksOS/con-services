namespace VSS.TRex.QuantizedMesh.Models
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
