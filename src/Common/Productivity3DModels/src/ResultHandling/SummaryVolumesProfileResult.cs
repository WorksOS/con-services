using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// Represents result returned by Summary Volumes Profile request
  /// </summary>
  public class SummaryVolumesProfileResult : ContractExecutionResult
  {
    public double GridDistanceBetweenProfilePoints { get; }

    public List<SummaryVolumesProfileCell> ProfileCells { get; }

    public bool HasData() => (ProfileCells?.Count ?? 0) > 0;

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private SummaryVolumesProfileResult()
    { }

    /// <summary>
    /// Override constructor with parameters.
    /// </summary>
    public SummaryVolumesProfileResult(double gridDistanceBetweenProfilePoints, List<SummaryVolumesProfileCell> profileCells)
    {
      GridDistanceBetweenProfilePoints = gridDistanceBetweenProfilePoints;
      ProfileCells = profileCells;
    }
  }
}
