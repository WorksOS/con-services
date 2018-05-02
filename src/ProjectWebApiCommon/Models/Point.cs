using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class Point : IEquatable<Point>
  {
    public double x;
    public double y;

    public Point(double latitude, double longitude)
    {
      this.x = longitude;
      this.y = latitude;
    }

    public double Latitude
    {
      get { return y; }
    }

    public double Longitude
    {
      get { return x; }
    }

    public bool Equals(Point other)
    {
      return other != null ? (this.Latitude == other.Latitude && this.Longitude == other.Longitude) : false;
    }
  }
}
