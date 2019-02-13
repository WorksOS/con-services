using System.Collections.Generic;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling.Profiling
{
  /// <summary>
  /// Represents result returned by Summary Volumes Profile request
  /// </summary>
  public class ProfileDataResult<T> : ContractExecutionResult
  {
    public double GridDistanceBetweenProfilePoints { get; }

    public List<T> ProfileCells { get; }

    public bool HasData() => (ProfileCells?.Count ?? 0) > 0;

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private ProfileDataResult()
    { }

    /// <summary>
    /// Override constructor with parameters.
    /// </summary>
    public ProfileDataResult(double gridDistanceBetweenProfilePoints, List<T> profileCells)
    {
      GridDistanceBetweenProfilePoints = gridDistanceBetweenProfilePoints;
      ProfileCells = profileCells;
    }
  }
}
