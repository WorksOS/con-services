using System;
using System.Collections.Generic;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Common.ResultHandling
{
  /// <summary>
  /// Represents result returned by Profile slicer request for compaction.
  /// </summary>
  public class CompactionProfileResult<T> : ContractExecutionResult
  {
    /// <summary>
    /// The grid distance between the two profile end points. For straight line profiles this is the geomtric plane distance between the points. 
    /// For alignment profiles this is the station distance between start and end locations on the alignment the profile is computed between.
    /// </summary>
    public double gridDistanceBetweenProfilePoints;

    /// <summary>
    /// The collection of results produced by the query. 
    /// </summary>
    public List<T> results;
  }
}