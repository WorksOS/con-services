using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  public class ProjectUidHelper
  {
    protected static ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();

    public static GetProjectAndAssetUidsResult FormatResult(string projectUid, string assetUid, int uniqueCode = 0)
    {
      return GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult(projectUid, assetUid,
        (uniqueCode <= 0 ? uniqueCode : contractExecutionStatesEnum.GetErrorNumberwithOffset(uniqueCode)),
        (uniqueCode == 0 ? ContractExecutionResult.DefaultMessage :
          (uniqueCode < 0 ? string.Empty : string.Format(contractExecutionStatesEnum.FirstNameWithOffset(uniqueCode)))));
    }

    public static GetProjectUidResult FormatResult(string projectUid, int uniqueCode = 0)
    {
      return GetProjectUidResult.CreateGetProjectUidResult(projectUid, 
        (uniqueCode <= 0 ? uniqueCode : contractExecutionStatesEnum.GetErrorNumberwithOffset(uniqueCode)),
        (uniqueCode == 0 ? ContractExecutionResult.DefaultMessage :
          (uniqueCode < 0 ? string.Empty : string.Format(contractExecutionStatesEnum.FirstNameWithOffset(uniqueCode)))));
    }
  }
}
