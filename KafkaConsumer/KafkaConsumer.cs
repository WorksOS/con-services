using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using VSS.GenericConfiguration;
using KafkaConsumer.Kafka;
using KafkaConsumer.Interfaces;
using Repositories;

namespace KafkaConsumer
{
  public class KafkaConsumer<T> : IKafkaConsumer<T>
  {
    private readonly ILogger log;
    private readonly IKafka kafkaDriver;
    private readonly IConfigurationStore configurationStore;
    private readonly IRepositoryFactory dbRepositoryFactory;
    private readonly IMessageTypeResolver messageResolver;

    private string topicName;
    private CancellationTokenSource stopToken;
    private int batchSize = 100;

    public KafkaConsumer(IConfigurationStore config, IKafka driver, IRepositoryFactory repositoryFactory, IMessageTypeResolver resolver, ILoggerFactory logger)
    {
      kafkaDriver = driver;
      configurationStore = config;
      dbRepositoryFactory = repositoryFactory;
      messageResolver = resolver;
      batchSize = configurationStore.GetValueInt("KAFKA_BATCH_SIZE");
      Console.WriteLine("KafkaConsumer:KAFKA_BATCH_SIZE " + batchSize);
      log = logger.CreateLogger<KafkaConsumer<T>>();
    }


    public void SetTopic(string topic)
    {
      topicName = topic;
      kafkaDriver.InitConsumer(configurationStore);
      log.LogDebug("KafkaConsumer: " + topic + configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX"));
      kafkaDriver.Subscribe(new List<string>() {(topic + configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX")).Trim()});
    }

    public Task StartProcessingAsync(CancellationTokenSource token)
    {
      log.LogDebug("KafkaConsumer: StartProcessingAsync");
      stopToken = token;
      return Task.Factory.StartNew(() =>
      {
        while (!token.IsCancellationRequested)
        {
          ProcessMessage();
        }
      }, TaskCreationOptions.LongRunning);

      log.LogDebug("KafkaConsumer: StartProcessingAsync has been cancelled");

    }

    /// <summary>
    ///  for tests
    /// </summary>
    public void StartProcessingSync()
    {
       ProcessMessage();
    }


    private int batchCounter = 0;
    private void ProcessMessage()
    {
//      log.LogTrace("Kafka Consuming");
      var messages = kafkaDriver.Consume(TimeSpan.FromMilliseconds(500));
      if (messages.message == Error.NO_ERROR)
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
            dbRepositoryFactory.GetRepository<T>().StoreEvent(deserializedObject);
          }
          catch (Exception ex)
          {
            log.LogDebug("KafkaConsumer: An unexpected error occured in KafkaConsumer: {0}; stacktrace: {1}", ex.Message, ex.StackTrace);
            if (ex.InnerException != null)
            {
              log.LogDebug("KafkaConsumer: Reason: {0}; stacktrace: {1}", ex.InnerException.Message, ex.InnerException.StackTrace);
            }
          }
          finally
          {
            log.LogDebug("Kafka Commiting");
            if (batchCounter > batchSize)
            {
              kafkaDriver.Commit();
              batchCounter = 0;
            }
            else
              batchCounter++;
          }
        }
      if (messages.message == Error.NO_DATA && batchCounter != 0)
      {
        kafkaDriver.Commit();
        batchCounter = 0;
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