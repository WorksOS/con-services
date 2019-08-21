using System;

namespace VSS.TRex.QuantizedMesh.Models
{
  /// <summary>
  /// 3D Vector Double Precision
  /// </summary>
  public struct Vector3
  {
    public Double X;
    public Double Y;
    public Double Z;
    public Vector3(Double x, Double y, Double z)
    {
      X = x;
      Y = y;
      Z = z;
    }
  }
}
