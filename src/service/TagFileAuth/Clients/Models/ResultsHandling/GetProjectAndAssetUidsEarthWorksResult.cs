using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.TagFileAuth.Models.ResultsHandling
{
  public class GetProjectAndAssetUidsEarthWorksResult : ContractExecutionResult, IMasterDataModel
  {
    /// <summary>
    /// The Uid of the project. empty if none.
    /// </summary>
    public string ProjectUid { get; set; }

    /// <summary>
    /// The Uid of asset if one matches ecSerial or radioSerial
    /// </summary>
    public string AssetUid { get; set; }

    /// <summary>
    /// The Uid of the customer if match by asset or tccOrgUid
    /// </summary>
    public string CustomerUid { get; set; }

    /// <summary>
    /// EarthWorks cutfill can return project, where no traditional sub is available
    /// tagFile endpoint will not return a project correct a sub is available
    /// </summary>
    public bool HasValidSub { get; set; } = false;

    /// <summary>
    /// Create instance of GetProjectAndAssetUidsEarthWorksResult
    ///    The Code is the unique code (or 0 for success) code to use for translations.
    ///       We re-purpose ContractExecutionResult.Code with this unique code.
    ///    For TFA, these are 3k based 
    ///    Message is the english verion of any error
    /// </summary>
    public GetProjectAndAssetUidsEarthWorksResult(string projectUid, string assetUid, string customerUid, bool hasValidSub, int uniqueCode = 0, string messageDetail = "success")
    {
      ProjectUid = projectUid;
      AssetUid = assetUid;
      CustomerUid = customerUid;
      HasValidSub = hasValidSub;
      Code = uniqueCode;
      Message = messageDetail;
    }

    public static GetProjectAndAssetUidsEarthWorksResult FormatResult(string projectUid = "", string assetUid = "", string customerUid = "", bool hasValidSub = false, int uniqueCode = 0)
    {
      var contractExecutionStatesEnum = new ContractExecutionStatesEnum();
      return new GetProjectAndAssetUidsEarthWorksResult(projectUid, assetUid, customerUid, hasValidSub,
        uniqueCode <= 0 ? uniqueCode : contractExecutionStatesEnum.GetErrorNumberwithOffset(uniqueCode),
        uniqueCode == 0 ? DefaultMessage :
          uniqueCode < 0 ? string.Empty : string.Format(contractExecutionStatesEnum.FirstNameWithOffset(uniqueCode)));
    }

    public List<string> GetIdentifiers() => string.IsNullOrEmpty(ProjectUid) ? new List<string>() : new List<string>() { ProjectUid };
  }
}
