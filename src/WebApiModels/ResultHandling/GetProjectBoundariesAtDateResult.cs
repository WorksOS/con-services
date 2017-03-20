
using WebApiModels.Models;

namespace WebApiModels.ResultHandling
{
  /// <summary>
  /// The result representation of a get project boundaries request.
  /// </summary>
  public class GetProjectBoundariesAtDateResult : ContractExecutionResult 
  {
    private ProjectBoundaryPackage[] _projectBoundaries;

    /// <summary>
    /// The boundaries of the projects. Empty if none.
    /// </summary>
    public ProjectBoundaryPackage[] projectBoundaries { get { return _projectBoundaries; } set { _projectBoundaries = value; } }  

    // acceptance tests cannot serialize with a private const.
    //private GetProjectBoundariesAtDateResult()
    //{ }

    /// <summary>
    /// Create instance of GetProjectBoundariesAtDateResult
    /// </summary>
    public static GetProjectBoundariesAtDateResult CreateGetProjectBoundariesAtDateResult(bool result, ProjectBoundaryPackage[] projectBoundaries)
    {
      return new GetProjectBoundariesAtDateResult
      {
        result = result,
        projectBoundaries = projectBoundaries
      };
    }
    
  }
}