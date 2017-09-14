using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Repositories;
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

      await RaptorValidateProjectSettings(request);

      var upsertProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = Guid.Parse(request.projectUid),
        Settings = request.settings,
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
        var projectSettings = await projectRepo.GetProjectSettings(request.projectUid).ConfigureAwait(false);
        result = ProjectSettingsResult.CreateProjectSettingsResult(request.projectUid, projectSettings.Settings);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69, e.Message);
      }
      return result;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new System.NotImplementedException();
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
          .ValidateProjectSettings(Guid.Parse(request.projectUid), request.settings, headers)
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

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 67, result.Code.ToString(),
          result.Message);
      }
      return;
    }
  }
}