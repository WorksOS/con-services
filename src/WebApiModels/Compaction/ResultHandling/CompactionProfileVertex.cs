namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// The data for a compaction profile. The cellType flag determines the type of cell. The cell edges or intersections are used for changing color.
  /// The midpoints are used for plotting or graphing the results as a profile line. 
  /// </summary>
  public class CompactionProfileVertex
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
  }
}