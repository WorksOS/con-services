using VSS.TRex.Common;

namespace VSS.TRex.Geometry
{
  /// <summary>
  /// A simple class containing a 3D point location expressed as double ordinates in the X, Y & Z axes
  /// </summary>
  public class FencePoint
  {
    /// <summary>
    /// X ordinate - Units are meters
    /// </summary>
    public double X { get; set; } = Consts.NullDouble;

    /// <summary>
    /// X Ordinate - Units are meters
    /// </summary>
    public double Y { get; set; } = Consts.NullDouble;

    /// <summary>
    /// Z Ordinate - Units are meters
    /// </summary>
    public double Z { get; set; } = Consts.NullDouble;

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public FencePoint()
    {
    }

    /// <summary>
    /// Constructor takes X and Y ordinate values
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public FencePoint(double x, double y) : base()
    {
      SetXY(x, y);
    }

    /// <summary>
    /// Constructor takes X, Y & Z ordinate values
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public FencePoint(double x, double y, double z) : base()
    {
      SetXYZ(x, y, z);
    }

    /// <summary>
    /// Constuctor taking another FencePoint as an argument. The result is a new clone of the FencePoint
    /// </summary>
    /// <param name="pt"></param>
    public FencePoint(FencePoint pt) : base()
    {
      Assign(pt);
    }

    /// <summary>
    /// Helper method to set they X and Y ordinate values in one step
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void SetXY(double x, double y)
    {
      X = x;
      Y = y;
      Z = Consts.NullDouble;
    }

    /// <summary>
    /// Helper method to set they X and Y ordinate values in one step
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void SetXYZ(double x, double y, double z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    /// <summary>
    /// Assign the state of source to this fence point instance
    /// </summary>
    /// <param name="source"></param>
    public void Assign(FencePoint source)
    {
      X = source.X;
      Y = source.Y;
      Z = source.Z;
    }

    /// <summary>
    /// Determines if this 3D point is the same as the point supplied in other
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(FencePoint other)
    {
      return X == other.X && Y == other.Y && Z == other.Z;
    }

    /// <summary>
    /// Determines if the 2D location of this point is the same as the point supplied in other
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool SameInPlan(FencePoint other)
    {
      return X == other.X && Y == other.Y;
    }

  }
}
