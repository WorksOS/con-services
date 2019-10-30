using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.TagFileAuth.Models
{
  public class ContractExecutionResultWithResult : ContractExecutionResult
  {
    protected static ContractExecutionStatesEnum _contractExecutionStatesEnum = new ContractExecutionStatesEnum();

    public bool Result { get; set; } = false;
  }
}
