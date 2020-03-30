using System.Collections.Generic;
using CCSS.Productivity3D.Preferences.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Handlers;
namespace CCSS.Productivity3D.Preferences.Common.Executors
{
  public class RequestExecutorContainerFactory
  {
    /// <summary>
    /// Builds this instance for specified executor type.
    /// </summary>
    /// <typeparam name="TExecutor">The type of the executor.</typeparam>
    /// <returns></returns>
    public static TExecutor Build<TExecutor>(
      ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler,
      string userId = null, IDictionary<string, string> headers = null,
      IPreferenceRepository preferenceRepo = null, IHttpContextAccessor httpContextAccessor = null
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
        log, serviceExceptionHandler, userId, headers, preferenceRepo, httpContextAccessor);

      return executor;
    }
  }
}
