namespace VSS.TRex.Designs.TTM.Optimised.Profiling
{
  /// <summary>
  /// Contains a 3D location and a station (distance from start point) for design profile results
  /// </summary>
  public struct XYZS
  {
    public double X, Y, Z, Station;

    public XYZS(double x, double y, double z, double station)
    {
      X = x;
      Y = y;
      Z = z;
      Station = station;
    }

    public XYZS(XYZS xyzs)
    {
      X = xyzs.X;
      Y = xyzs.Y;
      Z = xyzs.Z;
      Station = xyzs.Station;
    }

    public override string ToString() => $"X:{X:F3}, Y:{Y:F3}, Z:{Z:F3} Station:{Station:F3}";
  }
}
