using VSS.TRex.Designs.TTM;

namespace VSS.TRex.Exports.Surfaces.GridDecimator
{
  /// <summary>
  /// Representa a triangle via the plane equation derived from its vertices
  /// </summary>
  public class Plane
  {
    public double a, b, c;

    public Plane()
    {
    }

    public Plane(TriVertex v1, TriVertex v2, TriVertex v3)
    {
      Init(v1, v2, v3);
    }

    /// <summary>
    ///  Initialises the plane equation given X, Y, Z coordinates of three points of a triangle
    /// </summary>
    /// <param name="px"></param>
    /// <param name="py"></param>
    /// <param name="pz"></param>
    /// <param name="qx"></param>
    /// <param name="qy"></param>
    /// <param name="qz"></param>
    /// <param name="rx"></param>
    /// <param name="ry"></param>
    /// <param name="rz"></param>
    /// <returns></returns>
    public bool Init(double px, double py, double pz, double qx, double qy, double qz, double rx, double ry, double rz)
    {
      // find the plane z=ax+by+c passing through three points p,q,r

      // We explicitly declare these (rather than putting them in a
      // Vector) so that they can be allocated into registers.
      //Result := False;

      double ux = qx - px;
      double uy = qy - py;
      double uz = qz - pz;
      double vx = rx - px;
      double vy = ry - py;
      double vz = rz - pz;

      double den = ux * vy - uy * vx;
      if (den == 0)
        return false;

      a = (uz * vy - uy * vz) / den;
      b = (ux * vz - uz * vx) / den;
      c = pz - a * px - b * py;

      return true;
    }

    public bool Init(TriVertex v1, TriVertex v2, TriVertex v3) => Init(v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, v3.X, v3.Y, v3.Z);

    public double Evaluate(int x, int y) => a * x + b * y + c;

    public double Evaluate(double x, double y) => a * x + b * y + c;
  }
}
