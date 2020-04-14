using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.TagFileAuth.Models.ResultsHandling
{
  public class GetProjectAndAssetUidsResult : ContractExecutionResult, IMasterDataModel
  {
    /// <summary>
    /// The Uid of the project. empty if none.
    /// </summary>
    public string ProjectUid { get; set; }

    /// <summary>
    /// The Uid of the asset. Currently Trex thinks this is an AssetUid
    /// </summary>
    public string DeviceUid { get; set; }

    /// <summary>
    /// Create instance of GetProjectAndAssetUidsResult
    ///    The Code is the unique code (or 0 for success) code to use for translations.
    ///       We re-purpose ContractExecutionResult.Code with this unique code.
    ///    For TFA, these are 3k based 
    ///    Message is the english version of any error
    /// </summary>
    public GetProjectAndAssetUidsResult(string projectUid, string deviceUid, int uniqueCode = 0, string messageDetail = "success")
    {
      ProjectUid = projectUid;
      DeviceUid = deviceUid;
      Code = uniqueCode;
      Message = messageDetail;
    }

    public static GetProjectAndAssetUidsResult FormatResult(string projectUid = "", string assetUid = "", int uniqueCode = 0)
    {
      var contractExecutionStatesEnum = new ContractExecutionStatesEnum();
      return new GetProjectAndAssetUidsResult(projectUid, assetUid,
        uniqueCode <= 0 ? uniqueCode : contractExecutionStatesEnum.GetErrorNumberwithOffset(uniqueCode),
        uniqueCode == 0 ? DefaultMessage :
          uniqueCode < 0 ? string.Empty : string.Format(contractExecutionStatesEnum.FirstNameWithOffset(uniqueCode)));
    }

    public List<string> GetIdentifiers() => string.IsNullOrEmpty(ProjectUid) ? new List<string>() : new List<string>() { ProjectUid };
  }
}
