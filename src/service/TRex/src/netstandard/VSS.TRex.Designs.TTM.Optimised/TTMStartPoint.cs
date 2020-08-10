namespace VSS.TRex.Designs.TTM.Optimised
{
  /// <summary>
  /// The location of a start point on the TIN surface
  /// </summary>
  public struct TTMStartPoint
  {
    public double X;
    public double Y;
    public int Triangle;

    public static int SizeOf() => 2 * sizeof(double) * sizeof(int);
  }
}
