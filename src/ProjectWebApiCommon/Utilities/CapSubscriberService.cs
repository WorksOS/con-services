using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Repositories;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{
  public interface ISubscriberService
  {
    Task AddFileProcessed(AddFileResult result);
  }

  public class SubscriberService : ISubscriberService, ICapSubscribe
  {
    private readonly IProjectRepository projectRepo;
    private readonly IKafka producer;
    private readonly string kafkaTopicName;
    private readonly ILogger log;
    private readonly IServiceExceptionHandler serviceExceptionHandler;

    public SubscriberService(IProjectRepository projectRepo, IKafka producer, IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler)
    {
      this.serviceExceptionHandler = serviceExceptionHandler;
      this.projectRepo = projectRepo;
      this.producer = producer;

      kafkaTopicName = (configStore.GetValueString("PROJECTSERVICE_KAFKA_TOPIC_NAME") +
                        configStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX")).Trim();

      log = logger.CreateLogger<SubscriberService>();
    }

    [CapSubscribe("VSS.Productivity3D.Service.AddFileProcessedEvent-Dev")]
    [CapSubscribe("VSS.Productivity3D.Service.AddFileProcessedEvent-Alpha")]
    [CapSubscribe("VSS.Productivity3D.Service.AddFileProcessedEvent-3dpm")]
    public async Task AddFileProcessed(AddFileResult result)
    {
      log.LogInformation($"Received AddFileProcessedEvent from CAP for fileUid {result.FileUid}");
      var existing = await projectRepo.GetImportedFile(result.FileUid.ToString())
        .ConfigureAwait(false);

      if (existing == null)
      {
        log.LogWarning($"Failed to find file {result.FileUid} in database. Cannot update zoom levels.");
      }
      else
      {
        var updateImportedFileEvent = await ImportedFileRequestHelper.UpdateImportedFileInDb(existing,
            JsonConvert.SerializeObject(result.FileDescriptor),
            existing.SurveyedUtc, result.MinZoomLevel, result.MaxZoomLevel,
            existing.FileCreatedUtc, existing.FileUpdatedUtc, result.UserEmailAddress,
            log, serviceExceptionHandler, projectRepo)
          .ConfigureAwait(false);

        var messagePayload = JsonConvert.SerializeObject(new {UpdateImportedFileEvent = updateImportedFileEvent});
        producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>
          {
            new KeyValuePair<string, string>(updateImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
          });
      }
    }
  }
}
