using VSS.Common.ResultsHandling;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling
{
  public class ContractExecutionResultWithResult : ContractExecutionResult
  {
    protected static ContractExecutionStatesEnum _contractExecutionStatesEnum = new ContractExecutionStatesEnum();

    public bool Result { get; set; } = false;
  }
}
