using System.Collections.Generic;
using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  /// <summary>
  /// Represents result returned by Profile slicer request for compaction.
  /// </summary>
  public class CompactionProfileResult : ContractExecutionResult
  {
    /// <summary>
    /// The grid distance between the two profile end points. For straight line profiles this is the geomtric plane distance between the points. 
    /// For alignment profiles this is the station distance between start and end locations on the alignment the profile is computed between.
    /// </summary>
    public double gridDistanceBetweenProfilePoints;

    /// <summary>
    /// The collection of cells produced by the query. Cells are ordered by increasing station value along the line or alignment.
    /// </summary>
    public List<CompactionProfileCell> cells;
  }
}
