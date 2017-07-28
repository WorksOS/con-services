using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Repositories;
using VSS.MasterDataProxies.Interfaces;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  public class ProjectSettingsValidation
  {
    public static async Task RaptorValidateProjectSettings(IRaptorProxy raptorProxy,
      ILogger log,
      IServiceExceptionHandler serviceExceptionHandler,
      ProjectSettingsRequest request, IDictionary<string, string> customHeaders)
    {
      BaseDataResult result = null;
      try
      {
        result = await raptorProxy
          .ValidateProjectSettings(Guid.Parse(request.projectUid), request.settings, customHeaders)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"RaptorValidateProjectSettings: RaptorServices failed with exception. projectUid:{request.projectUid} settings:{request.settings}. Exception Thrown: {e.Message}. ");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 70,
          "raptorProxy.ValidateProjectSettings", e.Message);
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
      return;
    }

    /// <summary>
    /// Validates a project identifier.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <returns></returns>
    public static async Task ValidateProjectId(IProjectRepository projectRepo,
      ILogger log,
      IServiceExceptionHandler serviceExceptionHandler, string customerUid, string projectUid)
    {
      var project =
        (await projectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).FirstOrDefault(
          p => string.Equals(p.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase));
      if (project == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);
      }

      log.LogInformation($"projectUid {projectUid} validated");
    }
  }
}