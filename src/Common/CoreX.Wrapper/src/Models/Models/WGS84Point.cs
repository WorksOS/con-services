namespace CoreXModels
{
  public class WGS84Point
  {
    public double Lat;
    public double Lon;
    public double Height;

    public WGS84Point(double lon, double lat, double height)
    {
      Lat = lat;
      Lon = lon;
      Height = height;
    }

    public override string ToString() => $"Lat: {Lat}, Lon: {Lon}, Height: {Height}";
  }
}
