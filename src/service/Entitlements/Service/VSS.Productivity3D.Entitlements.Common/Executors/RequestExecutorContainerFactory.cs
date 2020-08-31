using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Entitlements.Abstractions.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Entitlements.Common.Executors
{
  public class RequestExecutorContainerFactory
  {
    /// <summary>
    /// Builds this instance for specified executor type.
    /// </summary>
    /// <typeparam name="TExecutor">The type of the executor.</typeparam>
    public static TExecutor Build<TExecutor>(
      ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler, ITPaaSApplicationAuthentication authn, IEmsClient emsClient
      )
      where TExecutor : RequestExecutorContainer, new()
    {
      ILogger log = null;
      if (logger != null)
      {
        log = logger.CreateLogger<RequestExecutorContainer>();
      }

      var executor = new TExecutor();

      executor.Initialise(
        log, serviceExceptionHandler, authn, emsClient);

      return executor;
    }
  }
}
