using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.TagFileAuth.Models
{
  public class GetProjectAndAssetUidsResult : ContractExecutionResult, IMasterDataModel
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
    /// CTCT cutfill can return project, where no traditional sub is available
    /// tagFile endpoint will not return a project correct a sub is available
    /// </summary>
    public bool HasValidSub { get; set; } = false;

    /// <summary>
    /// Create instance of GetProjectAndAssetUidsResult
    ///    The Code is the unique code (or 0 for success) code to use for translations.
    ///       We re-purpose ContractExecutionResult.Code with this unique code.
    ///    For TFA, these are 3k based 
    ///    Message is the english verion of any error
    /// </summary>
    public GetProjectAndAssetUidsResult(string projectUid, string assetUid, bool hasValidSub, int uniqueCode = 0, string messageDetail = "success")
    {
      ProjectUid = projectUid;
      AssetUid = assetUid;
      HasValidSub = hasValidSub;
      Code = uniqueCode;
      Message = messageDetail;
    }

    public List<string> GetIdentifiers() => string.IsNullOrEmpty(ProjectUid) ? new List<string>() : new List<string>(){ ProjectUid };
  }
}
