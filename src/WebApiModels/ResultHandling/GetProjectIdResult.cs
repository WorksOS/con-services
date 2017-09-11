
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
      int code = 0,
      int customCode = 0, string errorMessage1 = null, string errorMessage2 = null)
    {
      return new GetProjectIdResult
      {
        Result = result,
        projectId = projectId,
        Code = code,
        Message = code == 0 ? DefaultMessage : string.Format(_contractExecutionStatesEnum.FirstNameWithOffset(customCode), errorMessage1 ?? "null", errorMessage2 ?? "null")
      };
    }

  }
}