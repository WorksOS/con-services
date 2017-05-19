
using WebApiModels.Models;

namespace WebApiModels.ResultHandling
{
  /// <summary>
  /// The result representation of a get project boundaries request.
  /// </summary>
  public class GetProjectBoundariesAtDateResult : ContractExecutionResult
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
      ContractExecutionStatesEnum code = ContractExecutionStatesEnum.ExecutedSuccessfully,
      string message = "success")
    {
      return new GetProjectBoundariesAtDateResult
      {
        result = result,
        projectBoundaries = projectBoundaries,
        Code = code,
        Message = message
      };
    }

  }
}