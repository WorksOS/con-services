using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling
{
  public class ContractExecutionResultWithResult : ContractExecutionResult
  {
    protected static ContractExecutionStatesEnum _contractExecutionStatesEnum = new ContractExecutionStatesEnum();

    public bool Result { get; set; } = false;
  }
}
