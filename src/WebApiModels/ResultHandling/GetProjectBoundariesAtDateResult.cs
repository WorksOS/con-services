
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
    public static GetProjectBoundariesAtDateResult CreateGetProjectBoundariesAtDateResult(bool result, ProjectBoundaryPackage[] projectBoundaries)
    {
      return new GetProjectBoundariesAtDateResult
      {
        Result = result,
        projectBoundaries = projectBoundaries
      };
    }
    
  }
}