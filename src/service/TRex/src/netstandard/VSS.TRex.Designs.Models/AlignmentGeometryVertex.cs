using System;

namespace VSS.TRex.Designs.Models
{
  /// <summary>
  /// Contains a 3D location and a station (distance from start point) for design alignment geometry
  /// </summary>
  public class AlignmentGeometryVertex : IEquatable<AlignmentGeometryVertex>
  {
    /// <summary>
    /// 3D location plus distance from the start of the overall profile line
    /// </summary>
    public double X, Y, Z, Station;

    /// <summary>
    /// Constructs a new AlignmentGeometryVertex from the given location and station
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="station"></param>
    public AlignmentGeometryVertex(double x, double y, double z, double station)
    {
      X = x;
      Y = y;
      Z = z;
      Station = station;
    }

    /// <summary>
    /// Constructs a new AlignmentGeometryVertex as a clone of another AlignmentGeometryVertex instance
    /// </summary>
    /// <param name="vertex"></param>
    public AlignmentGeometryVertex(AlignmentGeometryVertex vertex)
    {
      X = vertex.X;
      Y = vertex.Y;
      Z = vertex.Z;
      Station = vertex.Station;
    }

    public override string ToString() => $"X:{X:F3}, Y:{Y:F3}, Z:{Z:F3} Station:{Station:F3}";

    public bool Equals(AlignmentGeometryVertex other)
    {
      return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && Station.Equals(other.Station);
    }
  }
}
