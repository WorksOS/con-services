using CoreX.Wrapper.Types;

namespace CoreX.Wrapper.Models
{
  public struct XYZ
  {
    public double X, Y, Z;

    public XYZ(double x, double y, double z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    public XYZ(double x, double y)
    {
      X = x;
      Y = y;
      Z = Consts.NULL_DOUBLE;
    }
  }
}
