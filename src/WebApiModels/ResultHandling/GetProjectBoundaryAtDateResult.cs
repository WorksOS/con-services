
using WebApiModels.Models;

namespace WebApiModels.ResultHandling
{
  /// <summary>
  /// The result representation of a get project boundary request.
  /// </summary>
  public class GetProjectBoundaryAtDateResult : ContractExecutionResult
  {
    /// <summary>
    /// The boundary of the project. Empty if none.
    /// </summary>
    public TWGS84FenceContainer projectBoundary { get; set; }

    /// <summary>
    /// Create instance of GetProjectBoundaryAtDateResult
    /// </summary>
    public static GetProjectBoundaryAtDateResult CreateGetProjectBoundaryAtDateResult(bool result, TWGS84FenceContainer projectBoundary)
    {
      return new GetProjectBoundaryAtDateResult
      {
        result = result,
        projectBoundary = projectBoundary
      };
    }
  }
}