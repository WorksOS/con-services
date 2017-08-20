namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// The data for a compaction profile. The cellType flag determines the type of cell. The cell edges or intersections are used for changing color.
  /// The midpoints are used for plotting or graphing the results as a profile line. 
  /// </summary>
  public class CompactionProfileVertex
  {
    /// <summary>
    /// The type of cell, either a cell edge intersection or the mid point of a segment cutting the cell. A edge can also be the start of a gap in the data.
    /// </summary>
    public ProfileCellType cellType;

    /// <summary>
    /// The station value, or distance from start of the profile line at which the profile line intersects this cell for cell edges 
    /// or the mid point of the line segment cutting through the cell for mid point type vertices.
    /// </summary>
    public double station;

    /// <summary>
    /// Elevation of the profile cell.
    /// </summary>
    public float elevation;

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="cell">The cell to copy</param>
    public CompactionProfileVertex(CompactionProfileVertex cell)
    {
      cellType = cell.cellType;
      station = cell.station;
      elevation = cell.elevation;
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionProfileVertex()
    { }

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
      Gap,
    }

    /// <summary>
    /// Specifies what the summary value represents in terms of the target
    /// </summary>
    public enum ValueTargetType
    {
      /// <summary>
      /// No value for this type of data for this cell
      /// </summary>
      NoData = -1,
      /// <summary>
      /// Value is above target
      /// </summary>
      AboveTarget = 0,
      /// <summary>
      /// Value is on target
      /// </summary>
      OnTarget = 1,
      /// <summary>
      /// Value is below target
      /// </summary>
      BelowTarget = 2
    }
  }
}