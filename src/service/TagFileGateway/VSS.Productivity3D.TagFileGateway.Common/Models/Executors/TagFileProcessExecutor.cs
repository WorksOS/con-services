using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.TagFileGateway.Common.Models.Executors
{
  public class TagFileProcessExecutor : RequestExecutorContainer
  {
    protected override Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new System.NotImplementedException();
    }
  }
}
