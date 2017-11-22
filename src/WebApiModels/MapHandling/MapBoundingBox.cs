namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// Model for map tile boundaing box. Lat/Lng are in radians.
  /// </summary>
  public class MapBoundingBox
  {
    public double minLat;
    public double minLng;
    public double maxLat;
    public double maxLng;

    public double centerLat => minLat + (maxLat - minLat) / 2;
    public double centerLng => minLng + (maxLng - minLng) / 2;
  }
}
