using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.Productivity3D.Preferences.Common.Utilities
{
  public static class LoggingExtensions
  {
    public static void LogResult(this ILogger log, string methodName, string request, ContractExecutionResult result)
    {
      if (result.Code == ContractExecutionStatesEnum.ExecutedSuccessfully)
      {
        var infoMessage = $"{methodName}: was successfully processed: Request {request} Result {JsonConvert.SerializeObject(result)}";
        log.LogInformation(infoMessage);
      }
      else
      {
        var errorMessage = $"{methodName}: failed to be processed: Request {request} Result {JsonConvert.SerializeObject(result)}";
        log.LogError(errorMessage);
      }
    }

  }
}
