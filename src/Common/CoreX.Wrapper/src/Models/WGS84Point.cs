namespace CoreX.Wrapper.Models
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
  }
}
