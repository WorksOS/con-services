using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Utilities
{
  public static class LoggingExtensions
  {
    public static int LogResult(this ILogger log, string methodName, Object request, ContractExecutionResult result)
    {
      var infoMessage = string.Format("{0}: was successfully processed: Request {1} Result {2}", methodName,
        JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(result));
      log.LogInformation(infoMessage);
      return 0;
    }
  }
}
