
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling
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
      ProjectBoundaryPackage[] projectBoundaries,
      int code = ContractExecutionStatesEnum.ExecutedSuccessfullyConst,
      string message = "success")
    {
      return new GetProjectBoundariesAtDateResult
      {
        Result = result,
        projectBoundaries = projectBoundaries,
        Code = code,
        Message = message
      };
    }

  }
}