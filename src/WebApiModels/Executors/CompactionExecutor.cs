using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using VSS.Raptor.Service.Common.Utilities;
using VSS.Raptor.Service.WebApiModels.Interfaces;
using VSS.Raptor.Service.WebApiModels.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Executors
{
  /// <summary>
  /// Processes the request to xxx
  /// </summary>
  public class CompactionExecutor : RequestExecutorContainer
  {
    private static readonly ILogger log =
      DependencyInjectionProvider.ServiceProvider.GetService<ILoggerFactory>().CreateLogger<CompactionExecutor>();

    /// <summary>
    ///   Processes the xxx request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item">Request to process</param>
    /// <returns>a xxxResult if successful</returns>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      return null;
    }
  }
}
