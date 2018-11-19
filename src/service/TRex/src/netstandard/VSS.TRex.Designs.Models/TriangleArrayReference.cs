namespace VSS.TRex.Designs
{
  /// <summary>
  /// Contains a reference into a TIN/triangle spatial index that is defined as a single list of triangle references for the entire design
  /// </summary>
  public struct TriangleArrayReference
  {
    public int TriangleArrayIndex;
    public short Count;
  }
}
