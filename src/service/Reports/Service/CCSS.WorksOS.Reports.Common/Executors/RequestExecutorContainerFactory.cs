using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.WorksOS.Reports.Common.Executors
{
  public static class RequestExecutorContainerFactory
  {
    /// <summary>
    /// Builds this instance for specified executor type.
    /// </summary>
    /// <typeparam name="TExecutor">The type of the executor.</typeparam>
    public static TExecutor Build<TExecutor>(
      ILoggerFactory logger, IConfigurationStore configStore, IServiceExceptionHandler serviceExceptionHandler,
      string customerUid, string userUid, IHeaderDictionary headers,
      IPreferenceProxy preferenceProxy, IWebRequest gracefulClient
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
        log, configStore, serviceExceptionHandler, customerUid, userUid, headers,
        preferenceProxy, gracefulClient
        );

      return executor;
    }
  }
}
