using System;
using System.Threading.Tasks;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Common.Executors
{
  /// <summary>
  /// Executor for ...
  /// </summary>
  public class DrainageExecutor : RequestExecutorContainer
  {
    public DrainageExecutor()
    {
      ProcessErrorCodes();
    }

    protected sealed override void ProcessErrorCodes()
    { }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as DrainageRequest;

      var configPathAndFilename = "..\\..\\..\\..\\..\\..\\TestData\\Sample\\TestCase.xml";
      //var useCase = TestCase.Load(configPathAndFilename);
      //if (useCase == null)
      //  throw new ArgumentException("Unable to load surface configuration");

      return new DrainageResult(String.Empty);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
