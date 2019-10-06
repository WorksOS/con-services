using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Morph.Services.Core.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace VSS.Hydrology.WebApi.Common.Executors
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
      string customerUid, string userId = null, string userEmailAddress = null, IDictionary<string, string> headers = null,
      ILandLeveling landLeveling = null, IProductivity3dV2ProxyCompaction productivity3dV2ProxyCompaction = null
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
        log, configStore, serviceExceptionHandler, 
        customerUid, userId, userEmailAddress, headers,
        landLeveling, productivity3dV2ProxyCompaction
        );

      return executor;
    }
  }
}
