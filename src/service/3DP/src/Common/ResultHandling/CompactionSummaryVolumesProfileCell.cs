namespace VSS.Productivity3D.Common.ResultHandling
{
  /// <summary>
  /// The data for a compaction summary volumes profile. The cellType flag determines the type of cell. The cell edges or intersections are used for changing color.
  /// The midpoints are used for plotting or graphing the results as a profile line. 
  /// </summary>
  public class CompactionSummaryVolumesProfileCell
  {
    /// <summary>
    /// The type of cell, either a cell edge intersection or the mid point of a segment cutting the cell. A edge can also be the start of a gap in the data.
    /// </summary>
    public ProfileCellType cellType;

    /// <summary>
    /// The station value, or distance from start of the profile line at which the profile line intersects this cell for cell edges 
    /// or the mid point of the line segment cutting through the cell for mid point type points.
    /// </summary>
    public double station;

    /// <summary>
    /// Elevation of the design at the location of the center point of the cell.
    /// </summary>
    public float designHeight;

    /// <summary>
    /// The elevation of the last (in time) cell pass involved in computation of this profile cell for the base filter (ground).
    /// </summary>
    public float lastPassHeight1;

    /// <summary>
    /// The elevation of the last (in time) cell pass involved in computation of this profile cell for the top filter (ground).
    /// </summary>
    public float lastPassHeight2;

    /// <summary>
    /// Cut-fill value in meters. The difference between the two ground elevations or the design and ground elevation depending on the summary volumes calculation type.
    /// </summary>
    public float cutFill;

    /// <summary>
    /// Default constructor
    /// </summary>
    public CompactionSummaryVolumesProfileCell()
    { }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="cell">The cell to copy</param>
    public CompactionSummaryVolumesProfileCell(CompactionSummaryVolumesProfileCell cell)
    {
      cellType = cell.cellType;
      station = cell.station;
      lastPassHeight1 = cell.lastPassHeight1;
      lastPassHeight2 = cell.lastPassHeight2;
      designHeight = cell.designHeight;
      cutFill = cell.cutFill;
    }
  }
}
