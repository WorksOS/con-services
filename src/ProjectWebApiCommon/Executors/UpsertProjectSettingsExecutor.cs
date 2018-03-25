using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
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
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ProjectSettingsResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      ContractExecutionResult result = null;

      ProjectSettingsRequest request = item as ProjectSettingsRequest;
      if (request == null)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68);
      await ValidateProjectWithCustomer(customerUid, request?.projectUid);

      if (request.ProjectSettingsType == ProjectSettingsType.Targets || request.ProjectSettingsType == ProjectSettingsType.Colors)
      {
        await RaptorValidateProjectSettings(request);
      }

      var upsertProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = Guid.Parse(request.projectUid),
        UserID = userId,
        ProjectSettingsType = request.ProjectSettingsType,
        Settings = request.Settings,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };

      if (await projectRepo.StoreEvent(upsertProjectSettingsEvent).ConfigureAwait(false) < 1)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 52);

      try
      {
        var messagePayload = JsonConvert.SerializeObject(new { UpdateProjectSettingsEvent = upsertProjectSettingsEvent });
        producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>
          {
            new KeyValuePair<string, string>(upsertProjectSettingsEvent.ProjectUID.ToString(), messagePayload)
          });
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 72, e.Message);
      }

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

    protected override void ProcessErrorCodes()
    {
    }

    private async Task RaptorValidateProjectSettings(ProjectSettingsRequest request)
    {
      BaseDataResult result = null;
      try
      {
        result = await raptorProxy
          .ValidateProjectSettings(request, headers)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"RaptorValidateProjectSettings: RaptorServices failed with exception. projectUid:{request.projectUid} settings:{request.Settings}. Exception Thrown: {e.Message}. ");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 70,
          "raptorProxy.ValidateProjectSettings", e.Message);
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