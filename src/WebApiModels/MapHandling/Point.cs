using System;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  public class Point : IEquatable<Point>
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

    #region Equality test
    public bool Equals(Point other)
    {
      if (other == null)
        return false;

      const double EPSILON = 0.000001;
  
      return Math.Abs(this.Latitude - other.Latitude) < EPSILON &&
             Math.Abs(this.Longitude - other.Longitude) < EPSILON;
    
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public static bool operator ==(Point a, Point b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(Point a, Point b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is Point && this == (Point)obj;
    }
    #endregion
  }
}
