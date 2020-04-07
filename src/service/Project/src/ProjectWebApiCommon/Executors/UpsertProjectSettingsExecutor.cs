using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which upserts the project settings for the project
  /// </summary>
  public class UpsertProjectSettingsExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the UpsertProjectSettings request
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      ContractExecutionResult result = null;

      var request = CastRequestObjectTo<ProjectSettingsRequest>(item, errorCode: 68);

      await ValidateProjectWithCustomer(customerUid, request?.projectUid);

      if (request.ProjectSettingsType == ProjectSettingsType.Targets || request.ProjectSettingsType == ProjectSettingsType.Colors)
      {
        await RaptorValidateProjectSettings(request);
      }

      var upsertProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = new Guid(request.projectUid),
        UserID = userId,
        ProjectSettingsType = request.ProjectSettingsType,
        Settings = request.Settings,
        ActionUTC = DateTime.UtcNow
      };

      if (await projectRepo.StoreEvent(upsertProjectSettingsEvent).ConfigureAwait(false) < 1)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 52);
      
      try
      {
        var projectSettings = await projectRepo.GetProjectSettings(request.projectUid, userId, request.ProjectSettingsType).ConfigureAwait(false);

        switch (request.ProjectSettingsType)
        {
          case ProjectSettingsType.Targets:
          case ProjectSettingsType.Colors:
            result = projectSettings == null ?
              ProjectSettingsResult.CreateProjectSettingsResult(request.projectUid, null, request.ProjectSettingsType) :
              ProjectSettingsResult.CreateProjectSettingsResult(request.projectUid, JsonConvert.DeserializeObject<JObject>(projectSettings.Settings), projectSettings.ProjectSettingsType);
            break;
          case ProjectSettingsType.ImportedFiles:
            var tempObj = JsonConvert.DeserializeObject<JArray>(projectSettings.Settings);
            var tempJObject = new JObject { ["importedFiles"] = tempObj };
            result = ProjectSettingsResult.CreateProjectSettingsResult(request.projectUid, tempJObject, projectSettings.ProjectSettingsType);
            break;
          default:
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 77);
            break;
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69, e.Message);
      }
      return result;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    private async Task RaptorValidateProjectSettings(ProjectSettingsRequest request)
    {
      BaseMasterDataResult result = null;
      try
      {
        result = await productivity3dV2ProxyCompaction
          .ValidateProjectSettings(request, customHeaders)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(e, $"RaptorValidateProjectSettings: RaptorServices failed with exception. projectUid:{request.projectUid} settings:{request.Settings}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 70,
          "productivity3dV2ProxyCompaction.ValidateProjectSettings", e.Message);
      }

      log.LogDebug(
        $"RaptorValidateProjectSettings: projectUid: {request.projectUid} settings: {request.Settings}. RaptorServices returned code: {result?.Code ?? -1} Message {result?.Message ?? "result == null"}.");

      if (result != null && result.Code != 0)
      {
        log.LogError(
          $"RaptorValidateProjectSettings: RaptorServices failed. projectUid:{request.projectUid} settings:{request.Settings}. Reason: {result?.Code ?? -1} {result?.Message ?? "null"}. ");

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 67, result.Code.ToString(),
          result.Message);
      }
    }
  }
}
