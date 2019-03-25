using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.AssetMgmt3D
{
  public class AssetMgmt3DExecutionStates : ContractExecutionStatesEnum
  {
    public enum ErrorCodes
    {
      GeneralError = 0,
      FilterConvertFailure = 1,
      DataError = 2,
    }

    public AssetMgmt3DExecutionStates()
    {
      DynamicAddwithOffset("Error: {0}{1}", (int)ErrorCodes.GeneralError);
      DynamicAddwithOffset("Failed to convert filter. Reason: {1}", (int)ErrorCodes.FilterConvertFailure);
      DynamicAddwithOffset("Failed to query data. Code: {0}, Message: {1}", (int)ErrorCodes.DataError);
    }
  }
}