using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;

namespace VSS.Raptor.Service.WebApiModels.Compaction.Executors
{
  /// <summary>
  /// Processes the request to xxx
  /// </summary>
  public class CompactionExecutor : RequestExecutorContainer
  {
    public CompactionExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CompactionExecutor()
    {
    }

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
