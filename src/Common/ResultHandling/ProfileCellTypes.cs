namespace VSS.Productivity3D.Common.ResultHandling
{
  /// <summary>
  /// Specifies the type of profile cell 
  /// </summary>
  public enum ProfileCellType
  {
    /// <summary>
    /// Station intersects the cell edge and has data
    /// </summary>
    Edge,

    /// <summary>
    /// Station is the midpoint of the line segment that cuts through the cell
    /// </summary>
    MidPoint,

    /// <summary>
    /// Station intersects the cell edge and has no data; the start of a gap
    /// </summary>
    Gap
  }
}