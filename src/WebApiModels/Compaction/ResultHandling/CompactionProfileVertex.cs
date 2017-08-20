namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// The data for a compaction profile. The cellType flag determines the type of cell. The cell edges or intersections are used for changing color.
  /// The midpoints are used for plotting or graphing the results as a profile line. 
  /// </summary>
  public class CompactionProfileVertex : ProfileResultBase
  {
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
    /// Default constructor.
    /// </summary>
    public CompactionProfileVertex()
    { }

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
  }
}