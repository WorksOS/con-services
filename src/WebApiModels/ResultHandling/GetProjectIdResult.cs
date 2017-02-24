using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.WebApiModels.ResultHandling
{
  /// <summary>
  /// The result representation of a get project id request.
  /// </summary>
  public class GetProjectIdResult : ContractExecutionResult
  {
    /// <summary>
    /// The result of the request. True for success and false for failure.
    /// </summary>
    public bool result { get; private set; }

    /// <summary>
    /// The id of the project. -1 if none.
    /// </summary>
    public long projectId { get; private set; }

    ///// <summary>
    ///// Private constructor
    ///// </summary>
    //private GetProjectIdResult()
    //{ }

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

    /// <summary>
    /// Example for Help
    /// </summary>
    public static GetProjectIdResult HelpSample
    {
      get { return CreateGetProjectIdResult(true, 284); }
    }
  }
}