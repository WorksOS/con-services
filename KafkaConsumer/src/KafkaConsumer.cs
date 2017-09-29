using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Interfaces;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Repositories;

namespace VSS.KafkaConsumer
{
  public class KafkaConsumer<T> : IKafkaConsumer<T>
  {
    private readonly ILogger log;
    private readonly ILoggerFactory LoggerFactory;
    private readonly IKafka kafkaDriver;
    private readonly IConfigurationStore configurationStore;
    private readonly IRepositoryFactory dbRepositoryFactory;
    private readonly IMessageTypeResolver messageResolver;

    private string topicName;
    private CancellationTokenSource stopToken;
    private int requestTime = 2000;

    public KafkaConsumer(IConfigurationStore config, IKafka driver, IRepositoryFactory repositoryFactory,
      IMessageTypeResolver resolver, ILoggerFactory logger)
    {
      kafkaDriver = driver;
      configurationStore = config;
      dbRepositoryFactory = repositoryFactory;
      messageResolver = resolver;
      if (configurationStore.GetValueInt("KAFKA_REQUEST_TIME") > 0)
        requestTime = configurationStore.GetValueInt("KAFKA_REQUEST_TIME");
      log = logger.CreateLogger<KafkaConsumer<T>>();
      LoggerFactory = logger;
    }


    public void SetTopic(string topic)
    {
      topicName = topic;
      kafkaDriver.InitConsumer(configurationStore, logger: LoggerFactory.CreateLogger<IKafka>());
      log.LogDebug("KafkaConsumer: " + topic + configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX"));
      kafkaDriver.Subscribe(new List<string>()
      {
        (topic + configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX")).Trim()
      });
    }

    public async Task<Task> StartProcessingAsync(CancellationTokenSource token)
    {
      log.LogDebug("KafkaConsumer: StartProcessingAsync");
      stopToken = token;

      return await Task.Factory.StartNew(async () =>
        {
          while (!token.IsCancellationRequested)
          {
            try
            {
              await ProcessMessage();
            }
            catch (Exception ex)
            {
              log.LogError($"Unhandled error occured {ex.Message} in {ex.StackTrace}");
            }
          }
        }, TaskCreationOptions.LongRunning)
        .ContinueWith((o) =>
        {
          log.LogWarning("KafkaConsumer: StartProcessingAsync has been cancelled");
          if (o.Exception != null)
          {
            log.LogCritical($"Exception: {o.Exception.Message}");
          }
          return Task.FromResult(1);
        });
    }

    /// <summary>
    ///  for tests
    /// </summary>
    public void StartProcessingSync()
    {
      ProcessMessage().Wait();
    }

    private async Task ProcessMessage()
    {
      var messages = kafkaDriver.Consume(TimeSpan.FromMilliseconds(requestTime));
      if (messages.message == Error.NO_ERROR)
      {
        foreach (var message in messages.payload)
        {
          try
          {
            string bytesAsString = Encoding.UTF8.GetString(message, 0, message.Length);
            //Debugging only
            log.LogDebug("KafkaConsumer: " + typeof(T) + " : " + bytesAsString);
            var deserializedObject = JsonConvert.DeserializeObject<T>(bytesAsString,
              messageResolver.GetConverter<T>());
            log.LogDebug("KafkaConsumer: Saving");
            await dbRepositoryFactory.GetRepository<T>().StoreEvent(deserializedObject);
          }
          catch (Exception ex)
          {
            log.LogError("KafkaConsumer: An unexpected error occured in KafkaConsumer: {0}; stacktrace: {1}",
              ex.Message, ex.StackTrace);
            if (ex.InnerException != null)
            {
              log.LogError("KafkaConsumer: Reason: {0}; stacktrace: {1}", ex.InnerException.Message,
                ex.InnerException.StackTrace);
            }
          }
        }
        log.LogDebug("Kafka Commiting " + "Partition " + messages.partition + " Offset: " + messages.offset);
        await kafkaDriver.Commit();
      }
    }

    public void StopProcessing()
    {
      stopToken.Cancel();
    }

    public void Dispose()
    {
      kafkaDriver.Dispose();
    }

  }
}