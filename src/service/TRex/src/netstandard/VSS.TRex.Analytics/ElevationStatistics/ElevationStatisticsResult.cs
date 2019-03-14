using VSS.TRex.Analytics.Foundation.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.Analytics.ElevationStatistics
{
  /// <summary>
  /// The result obtained from performing a Elevation analytics request
  /// </summary>
  public class ElevationStatisticsResult : AnalyticsResult
  {
    /// <summary>
    /// The minimum elevation value of the site model. 
    /// </summary>
    public double MinElevation { get; set; }

    /// <summary>
    /// The maximum elevation value of the site model.
    /// </summary>
    public double MaxElevation { get; set; }

    /// <summary>
    /// The area of cells that we have considered and successfully computed information from.
    /// </summary>
    public double CoverageArea { get; set; }

    /// <summary>
    /// The bounding extents of the computed area.
    /// </summary>
    public BoundingWorldExtent3D BoundingExtents { get; set; }
  }
}
