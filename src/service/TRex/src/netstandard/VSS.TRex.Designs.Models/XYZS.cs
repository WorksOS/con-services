using System;

namespace VSS.TRex.Designs.Models
{
  /// <summary>
  /// Contains a 3D location and a station (distance from start point) for design profile results
  /// </summary>
  public struct XYZS : IEquatable<XYZS>
  {
    /// <summary>
    /// 3D location plus distance from the start of the overall profile line
    /// </summary>
    public double X, Y, Z, Station;

    public int TriIndex;

    /// <summary>
    /// Constructs a new XYZS from the given location, station and triangle information
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="station"></param>
    /// <param name="triIndex"></param>
    public XYZS(double x, double y, double z, double station, int triIndex)
    {
      X = x;
      Y = y;
      Z = z;
      Station = station;
      TriIndex = triIndex;
    }

    /// <summary>
    /// Constructs a new XYZS as a clone of another XYZS instance
    /// </summary>
    /// <param name="xyzs"></param>
    public XYZS(XYZS xyzs)
    {
      X = xyzs.X;
      Y = xyzs.Y;
      Z = xyzs.Z;
      Station = xyzs.Station;
      TriIndex = xyzs.TriIndex;
    }

    public override string ToString() => $"X:{X:F3}, Y:{Y:F3}, Z:{Z:F3} Station:{Station:F3}, TriIndex:{TriIndex}";

    public bool Equals(XYZS other)
    {
      return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && Station.Equals(other.Station) && TriIndex == other.TriIndex;
    }
  }
}
