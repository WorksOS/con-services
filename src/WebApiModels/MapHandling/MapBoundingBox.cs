namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
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
