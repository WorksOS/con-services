namespace CoreX.Wrapper.Models
{
  public struct XYZ
  {
    public double X, Y, Z;

    private const double NULL_DOUBLE = 1E308;

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
      Z = NULL_DOUBLE;
    }
  }
}
