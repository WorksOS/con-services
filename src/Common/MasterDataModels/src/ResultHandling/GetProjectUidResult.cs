using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Models.ResultHandling
{
  public class GetProjectUidResult : ContractExecutionResult
  {
    /// <summary>
    /// The Uid of the project. emptry if none.
    /// </summary>
    public string ProjectUid { get; set; }

    /// <summary>
    /// Create instance of GetProjectUidResult
    ///    The Code is the unique code (or 0 for success) code to use for translations.
    ///       We re-purpose ContractExecutionResult.Code with this unique code.
    ///    For TFA, these are 3k based 
    ///    Message is the english verion of any error
    /// </summary>
    public static GetProjectUidResult CreateGetProjectUidResult(string projectUid, int uniqueCode = 0, string messageDetail = null)
    {
      return new GetProjectUidResult
      {
        ProjectUid = projectUid,
        Code = uniqueCode,
        Message = messageDetail
      };
    }
  }
}