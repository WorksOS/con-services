
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling
{
  /// <summary>
  /// The result representation of a get project boundary request.
  /// </summary>
  public class GetProjectBoundaryAtDateResult : ContractExecutionResultWithResult
  {
    /// <summary>
    /// The boundary of the project. Empty if none.
    /// </summary>
    public TWGS84FenceContainer projectBoundary { get; set; }

    /// <summary>
    /// Create instance of GetProjectBoundaryAtDateResult
    /// </summary>
    public static GetProjectBoundaryAtDateResult CreateGetProjectBoundaryAtDateResult(bool result,
      TWGS84FenceContainer projectBoundary,
      int code = 0,
      string message = "success")
    {
      return new GetProjectBoundaryAtDateResult
      {
        Result = result,
        projectBoundary = projectBoundary,
        Code = code,
        Message = message
      };
    }
  }
}