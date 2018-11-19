using VSS.TRex.Designs.TTM;

namespace VSS.TRex.Exports.Surfaces.GridDecimator
{
  /// <summary>
  /// Represents a triangle via the plane equation derived from its vertices
  /// </summary>
  public class Plane : VSS.TRex.Geometry.Plane
  {
    public Plane()
    { }

    public Plane(TriVertex v1, TriVertex v2, TriVertex v3)
    {
      Init(v1, v2, v3);
    }

    public bool Init(TriVertex v1, TriVertex v2, TriVertex v3) => Init(v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, v3.X, v3.Y, v3.Z);
  }
}
