using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Project.WebAPI.Common.Internal
{
  public static class LoggingExtensions
  {
    public static int LogResult(this ILogger log, string methodName, string projectUid, ContractExecutionResult result)
    {
      if (result.Code == ContractExecutionStatesEnum.ExecutedSuccessfully)
      {
        var infoMessage = string.Format("{0}: was successfully processed: Request {1} Result {2}", methodName, projectUid, JsonConvert.SerializeObject(result));
        log.LogInformation(infoMessage);
      }
      else
      {
        var errorMessage = string.Format("{0}: failed to be processed: Request {1} Result {2}", methodName, projectUid, JsonConvert.SerializeObject(result));
        log.LogError(errorMessage);
      }
      return 0;
    }

  }
}