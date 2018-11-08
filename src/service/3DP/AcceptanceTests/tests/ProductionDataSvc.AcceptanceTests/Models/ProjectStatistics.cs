using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// A representation of a set of spatial and temporal stastics for the project as a whole
  /// </summary>
  public class ProjectStatistics : ResponseBase
  {
    #region Members
    /// <summary>
    /// Earlist time stamped data present in the project, including both production and surveyed surface data.
    /// </summary>
    public DateTime startTime { get; set; }

    /// <summary>
    /// Latest time stamped data present in the project, including both production and surveyed surface data.
    /// </summary>
    public DateTime endTime { get; set; }

    /// <summary>
    /// Size of spatial data cells in the project (the default value is 34cm)
    /// </summary>
    public double cellSize { get; set; }

    /// <summary>
    /// The index origin offset from the absolute bottom left origin of the subgrid tree cartesian coordinate system to the centered origin of the cartesian
    /// grid coordinate system used in the project, and the centered origin cartesian coordinates of cell addresses.
    /// </summary>
    public int indexOriginOffset { get; set; }

    /// <summary>
    /// The three dimensional extents of the project including both production and surveyed surface data.
    /// </summary>
    public BoundingBox3DGrid extents { get; set; }
    #endregion

    public ProjectStatistics()
      : base("success")
    { }
  }
}
