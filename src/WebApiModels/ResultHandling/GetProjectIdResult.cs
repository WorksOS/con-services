
namespace WebApiModels.ResultHandling
{
  /// <summary>
  /// The result representation of a get project id request.
  /// </summary>
  public class GetProjectIdResult : ContractExecutionResult
  {
    /// <summary>
    /// The id of the project. -1 if none.
    /// </summary>
    public long projectId { get; set; }

    /// <summary>
    /// Create instance of GetProjectIdResult
    /// </summary>
    public static GetProjectIdResult CreateGetProjectIdResult(bool result, long projectId,
      ContractExecutionStatesEnum code = ContractExecutionStatesEnum.ExecutedSuccessfully,
      string message = "success")
    {
      return new GetProjectIdResult
      {
        result = result,
        projectId = projectId,
        Code = code,
        Message = message
      };
    }

  }
}