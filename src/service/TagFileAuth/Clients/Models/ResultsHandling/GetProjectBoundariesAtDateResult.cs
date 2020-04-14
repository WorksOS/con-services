using System.Collections.Generic;

namespace VSS.Productivity3D.TagFileAuth.Models.ResultsHandling
{

  /// <summary>
  /// The result representation of a get project boundaries request.
  /// </summary>
  public class GetProjectBoundariesAtDateResult : ContractExecutionResultWithResult
  {
    /// <summary>
    /// The boundaries of the projects. Empty if none.
    /// </summary>

    public ProjectBoundaryPackage[] projectBoundaries { get; set; }

    /// <summary>
    /// Create instance of GetProjectBoundariesAtDateResult
    /// </summary>
    public static GetProjectBoundariesAtDateResult CreateGetProjectBoundariesAtDateResult(bool result,
      List<ProjectBoundaryPackage> projectBoundaries,
      int code = 0,
      int customCode = 0, string errorMessage1 = null, string errorMessage2 = null)
    {
      return new GetProjectBoundariesAtDateResult
      {
        Result = result,
        projectBoundaries = projectBoundaries.ToArray(),
        Code = code,
        Message = code == 0 ? DefaultMessage : string.Format(_contractExecutionStatesEnum.FirstNameWithOffset(customCode), errorMessage1 ?? "null", errorMessage2 ?? "null")
      };
    }

  }
}
