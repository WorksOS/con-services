using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using KafkaConsumer.Kafka;
using MasterDataProxies.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositories;
using VSS.GenericConfiguration;
using VSS.Productivity3D.ProjectWebApiCommon.Internal;
using VSS.Productivity3D.ProjectWebApiCommon.Models;
using VSS.Productivity3D.ProjectWebApiCommon.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.ProjectWebApiCommon.Executors
{
  /// <summary>
  /// The executor which upserts the project settings for the project
  /// </summary>
  public class UpsertProjectSettingsExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public UpsertProjectSettingsExecutor(IRepository<IProjectEvent> projectRepo, IRaptorProxy raptorProxy, ILoggerFactory logger, IConfigurationStore configStore, IServiceExceptionHandler serviceExceptionHandler, IDictionary<string, string> customHeaders, IKafka producer) : base(projectRepo, raptorProxy, configStore, logger, serviceExceptionHandler, customHeaders, producer)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public UpsertProjectSettingsExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new System.NotImplementedException();
    }

    /// <summary>
    /// Processes the UpsertProjectSettings request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ProjectSettingsResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      ContractExecutionResult result = null;
      try
      {
        ProjectSettingsRequest request = item as ProjectSettingsRequest;

        await RaptorUpsertProjectSettings(request.projectUid, request.settings);

        var upsertProjectSettingsEvent = new UpdateProjectSettingsEvent()
        {
          ProjectUID = Guid.Parse(request.projectUid),
          Settings = request.settings,
          ActionUTC = DateTime.UtcNow,
          ReceivedUTC = DateTime.UtcNow
        };

        if (await projectRepo.StoreEvent(upsertProjectSettingsEvent).ConfigureAwait(false) < 1)
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 52);

        var messagePayload = JsonConvert.SerializeObject(new { UpdateProjectSettingsEvent = upsertProjectSettingsEvent });
        producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>
          {
            new KeyValuePair<string, string>(upsertProjectSettingsEvent.ProjectUID.ToString(), messagePayload)
          });

        var projectSettings = await projectRepo.GetProject(request.projectUid).ConfigureAwait(false);
        result = ProjectSettingsResult.CreateProjectSettingsResult(request.projectUid, request.settings);
      }
      catch( Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69, e.Message);
      }
      return result;
    }

    protected override void ProcessErrorCodes()
    {
    }

    protected async Task RaptorUpsertProjectSettings(string projectUid, string settings)
    {
      MasterDataProxies.ResultHandling.ContractExecutionResult result = null;
      try
      {
        // todo
        //result = await raptorProxy
        //  .ProjectSettingsUpsert(projectUid, settings, customHeaders)
        //  .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"ProjectSettingsUpsert: RaptorServices failed with exception. projectUid:{projectUid} settings:{settings}. Exception Thrown: {e.Message}. ");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 70, "raptorProxy.ProjectSettingsValidate", e.Message);
      }

      log.LogDebug(
        $"ProjectSettingsUpsert: projectUid: {projectUid} settings: {settings}. RaptorServices returned code: {result?.Code ?? -1} Message {result?.Message ?? "result == null"}.");

      if (result != null && result.Code != 0)
      {
        log.LogError($"ProjectSettingsUpsert: RaptorServices failed. projectUid:{projectUid} settings:{settings}. Reason: {result?.Code ?? -1} {result?.Message ?? "null"}. ");

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 67, result.Code.ToString(), result.Message);
      }
    }

  }
}