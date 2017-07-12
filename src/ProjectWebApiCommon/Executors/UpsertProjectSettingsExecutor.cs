using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using KafkaConsumer.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.GenericConfiguration;
using VSS.Productivity3D.ProjectWebApiCommon.Internal;
using VSS.Productivity3D.ProjectWebApiCommon.Models;
using VSS.Productivity3D.ProjectWebApiCommon.ResultsHandling;
using VSS.Productivity3D.Repo;
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
    public UpsertProjectSettingsExecutor(IProjectRepository projectRepo, ILoggerFactory logger, IConfigurationStore configStore, IServiceExceptionHandler serviceExceptionHandler, IKafka producer) : base(projectRepo, configStore, logger, serviceExceptionHandler, producer)
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

      ProjectSettingsRequest request = item as ProjectSettingsRequest;

      var upsertProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = Guid.Parse(request.projectUid),
        Settings = request.settings,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };

      if ( await projectRepo.StoreEvent(upsertProjectSettingsEvent).ConfigureAwait(false) < 1)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 52);

      try
      {
        var messagePayload = JsonConvert.SerializeObject(new {UpdateProjectSettingsEvent = upsertProjectSettingsEvent});
        producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>
          {
            new KeyValuePair<string, string>(upsertProjectSettingsEvent.ProjectUID.ToString(), messagePayload)
          });

        var projectSettings = await projectRepo.GetProjectSettings(request.projectUid).ConfigureAwait(false);
        result = ProjectSettingsResult.CreateProjectSettingsResult(request.projectUid, projectSettings.Settings);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69, e.Message);
      }
      return result;
    }

    protected override void ProcessErrorCodes()
    {
    }
    
  }
}