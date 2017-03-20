
namespace WebApiModels.ResultHandling
{
  /// <summary>
  /// The result representation of a get project id request.
  /// </summary>
  public class GetProjectIdResult : ContractExecutionResult
  {
    private long _projectId;

    /// <summary>
    /// The id of the project. -1 if none.
    /// </summary>
    public long projectId { get { return _projectId; } set { _projectId = value; } }

    /// <summary>
    /// Create instance of GetProjectIdResult
    /// </summary>
    public static GetProjectIdResult CreateGetProjectIdResult(bool result, long projectId)
    {
      return new GetProjectIdResult
      {
        result = result,
        projectId = projectId
      };
    }
    
  }
}