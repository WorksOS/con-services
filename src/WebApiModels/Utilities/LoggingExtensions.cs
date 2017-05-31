using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApiModels.Models;
using WebApiModels.ResultHandling;

namespace WebApiModels.Utilities
{
  public static class LoggingExtensions
  {

    public static int LogResult(this ILogger log, string methodName, ContractRequest request, ContractExecutionResult result)
    {
      if (result.Result)
      {
        var infoMessage = string.Format("{0}: was successfully processed: Request {1} Result {2}", methodName, JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(result));
        log.LogInformation(infoMessage);
      }
      else
      {
        var errorMessage = string.Format("{0}: failed to be processed: Request {1} Result {2}", methodName, JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(result));
        log.LogError(errorMessage);
      }
      return 0;
    }

  }
}