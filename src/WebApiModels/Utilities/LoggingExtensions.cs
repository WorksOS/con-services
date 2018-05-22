using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Utilities
{
  public static class LoggingExtensions
  {
    public static int LogResult(this ILogger log, string methodName, ContractRequest request, ContractExecutionResult result)
    {
      if ((result is ContractExecutionResultWithUniqueResultCode resCode && resCode.Code == 0) ||
          (result is ContractExecutionResultWithResult resResult && resResult.Result))
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