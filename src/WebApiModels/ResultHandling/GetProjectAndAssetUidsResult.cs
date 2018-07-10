
namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling
{
  /// <summary>
  /// The result representation of a get project and asset Uids request.
  /// </summary>
  public class GetProjectAndAssetUidsResult : ContractExecutionResultWithUniqueResultCode
  {
    /// <summary>
    /// The Uid of the project. empty if none.
    /// </summary>
    public string ProjectUid { get; set; }

    /// <summary>
    /// The Uid of the asset. could be empty/-1/-2?
    /// </summary>
    public string AssetUid { get; set; }

    /// <summary>
    /// Create instance of GetProjectAndAssetUidsResult
    ///    The Code is the unique code (or 0 for success) code to use for translations.
    ///       We re-purpose ContractExecutionResult.Code with this unique code.
    ///    For TFA, these are 3k based 
    ///    Message is the english verion of any error
    /// </summary>
    public static GetProjectAndAssetUidsResult CreateGetProjectAndAssetUidsResult(string projectUid, string assetUid, int uniqueCode = 0, string messageDetail = null)
    {
      return new GetProjectAndAssetUidsResult
      {
        ProjectUid = projectUid,
        AssetUid = assetUid,
        Code = uniqueCode == 0 ? uniqueCode : ContractExecutionStatesEnum.GetErrorNumberwithOffset(uniqueCode),
        Message = uniqueCode == 0 ? DefaultMessage : string.Format(ContractExecutionStatesEnum.FirstNameWithOffset(uniqueCode), messageDetail)        
      };
    }

  }
}