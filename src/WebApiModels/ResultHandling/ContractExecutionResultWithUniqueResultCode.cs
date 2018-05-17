using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling
{
  public class ContractExecutionResultWithUniqueResultCode : ContractExecutionResult
  {
    protected static ContractExecutionStatesEnum ContractExecutionStatesEnum = new ContractExecutionStatesEnum();
  }
}
