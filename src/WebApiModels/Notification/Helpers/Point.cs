namespace VSS.Productivity3D.WebApi.Models.Notification.Helpers
{
  public class Point
  {
    public double x;
    public double y;

    public Point()
    { }

    public Point(double latitude, double longitude)
    {
      x = longitude;
      y = latitude;
    }

    public double Latitude => y;

    public double Longitude => x;
  }
}