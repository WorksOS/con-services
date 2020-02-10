using System.Collections.Generic;
using CCSS.TagFileSplitter.WebAPI.Common.Models;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.TagFileSplitter.WebAPI.Common.Executors
{
  public class RequestExecutorContainerFactory
  {
    /// <summary>
    /// Builds this instance for specified executor type.
    /// </summary>
    /// <typeparam name="TExecutor">The type of the executor.</typeparam>
    /// <returns></returns>
    public static TExecutor Build<TExecutor>(
      ILoggerFactory logger, IConfigurationStore configStore, IServiceExceptionHandler serviceExceptionHandler, 
      IServiceResolution serviceResolution, IGenericHttpProxy genericHttpProxy, IDictionary<string, string> customHeaders,
      TargetServices targetServices, int? timeoutSeconds, string userEmailAddress = null)
      where TExecutor : RequestExecutorContainer, new()
    {
      ILogger log = null;
      if (logger != null)
      {
        log = logger.CreateLogger<RequestExecutorContainer>();
      }

      var executor = new TExecutor();

      executor.Initialise(
        log, configStore, serviceExceptionHandler,
        serviceResolution, genericHttpProxy, customHeaders,
        targetServices, timeoutSeconds, userEmailAddress);

      return executor;
    }
  }
}
