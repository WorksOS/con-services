
using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling
{
  /// <summary>
  /// The result representation of a get project id request.
  /// </summary>
  public class GetProjectIdResult : ContractExecutionResultWithResult
  {
    /// <summary>
    /// The id of the project. -1 if none.
    /// </summary>
    public long projectId { get; set; }

    /// <summary>
    /// Create instance of GetProjectIdResult
    /// </summary>
    public static GetProjectIdResult CreateGetProjectIdResult(bool result, long projectId,
      int code = ContractExecutionStatesEnum.ExecutedSuccessfullyConst,
      string message = "success")
    {
      return new GetProjectIdResult
      {
        Result = result,
        projectId = projectId,
        Code = code,
        Message = message
      };
    }

  }
}