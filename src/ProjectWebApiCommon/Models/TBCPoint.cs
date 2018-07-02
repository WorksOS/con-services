using System;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class TBCPoint : IEquatable<TBCPoint>
  {
    public double x;
    public double y;

    public TBCPoint(double latitude, double longitude)
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

    public bool Equals(TBCPoint other)
    {
      return other != null ? (this.Latitude == other.Latitude && this.Longitude == other.Longitude) : false;
    }
  }
}

