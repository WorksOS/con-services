using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VSS.Productivity3D.MasterDataProxies.Interfaces;
using VSS.Productivity3D.MasterDataProxies.ResultHandling;
using VSS.Productivity3D.ProjectWebApiCommon.Internal;
using VSS.Productivity3D.ProjectWebApiCommon.Models;

namespace VSS.Productivity3D.ProjectWebApiCommon.Executors
{
  public class ProjectSettingsValidation
  {

    public static async Task<ContractExecutionResult> RaptorValidateProjectSettings(IRaptorProxy raptorProxy,
      ILogger log,
      IServiceExceptionHandler serviceExceptionHandler,
      ProjectSettingsRequest request, IDictionary<string, string> customHeaders)
    {
      ContractExecutionResult result = null;
      try
      {
        // todo include projectUid
        result = await raptorProxy
          .ProjectSettingsValidate(Guid.Parse(request.projectUid), request.settings, customHeaders)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"RaptorValidateProjectSettings: RaptorServices failed with exception. projectUid:{request.projectUid} settings:{request.settings}. Exception Thrown: {e.Message}. ");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 70,
          "raptorProxy.ProjectSettingsValidate", e.Message);
      }

      log.LogDebug(
        $"RaptorValidateProjectSettings: projectUid: {request.projectUid} settings: {request.settings}. RaptorServices returned code: {result?.Code ?? -1} Message {result?.Message ?? "result == null"}.");

      if (result != null && result.Code != 0)
      {
        log.LogError(
          $"RaptorValidateProjectSettings: RaptorServices failed. projectUid:{request.projectUid} settings:{request.settings}. Reason: {result?.Code ?? -1} {result?.Message ?? "null"}. ");

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 67, result.Code.ToString(),
          result.Message);
      }
      return result;
    }

  }
}