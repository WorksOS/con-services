
namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling
{
  /// <summary>
  /// The result representation of a get project Uid request.
  /// </summary>
  public class GetProjectUidResult : ContractExecutionResultWithUniqueResultCode
  {
    /// <summary>
    /// The Uid of the project. emptry if none.
    /// </summary>
    public string ProjectUid { get; set; }

    /// <summary>
    /// Create instance of GetProjectUidResult
    ///    The Code is the unique code (or 0 for success) code to use for translations.
    ///       We re-purpose ContractExecutionResult.Code with this unique code.
    ///    For TFA, these are 2k based 
    ///    Message is the english verion of any error
    /// </summary>
    public static GetProjectUidResult CreateGetProjectUidResult(string projectUid, int uniqueCode = 0)
    {
      return new GetProjectUidResult
      {
        ProjectUid = projectUid,
        Code = ContractExecutionStatesEnum.GetErrorNumberwithOffset(uniqueCode),
        Message = uniqueCode == 0 ? DefaultMessage : string.Format(ContractExecutionStatesEnum.FirstNameWithOffset(uniqueCode))        
      };
    }

  }
}