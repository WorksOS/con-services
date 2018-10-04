using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Models.ResultHandling
{
  public class GetProjectAndAssetUidsResult : ContractExecutionResult
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
        Code = uniqueCode,
        Message = messageDetail
      };
    }
  }
}