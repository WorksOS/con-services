using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nito.AsyncEx.Synchronous;
using VSS.Common.Abstractions.Configuration;
using VSS.KafkaConsumer.Interfaces;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4NetExtensions;
using VSS.MasterData.Repositories;

namespace VSS.KafkaConsumer
{
  public class KafkaConsumer<T> : IKafkaConsumer<T>
  {
    private ILogger _log;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IKafka _kafkaDriver;
    private readonly IConfigurationStore _configurationStore;
    private readonly IRepositoryFactory _dbRepositoryFactory;
    private readonly IMessageTypeResolver _messageResolver;

    private string _topicName;
    private CancellationTokenSource _stopToken;
    private readonly int _requestTime = 1000;

    public KafkaConsumer(IConfigurationStore config, IKafka driver, IRepositoryFactory repositoryFactory,
      IMessageTypeResolver resolver, ILoggerFactory logger)
    {
      _kafkaDriver = driver;
      _configurationStore = config;
      _dbRepositoryFactory = repositoryFactory;
      _messageResolver = resolver;
      if (_configurationStore.GetValueInt("KAFKA_REQUEST_TIME") > 0)
        _requestTime = _configurationStore.GetValueInt("KAFKA_REQUEST_TIME");
      _log = logger.CreateLogger<KafkaConsumer<T>>();
      _loggerFactory = logger;
    }

    public void OverrideLogger(ILogger logger)
    {
      _log = logger;
    }


    public void SetTopic(string topic, string consumerGroup = null)
    {
      _topicName = topic;
      _kafkaDriver.InitConsumer(_configurationStore, groupName: consumerGroup,
        logger: _loggerFactory.CreateLogger<IKafka>());
      _log.LogDebug($"{nameof(SetTopic)}: {topic} {_configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX")}");
      _kafkaDriver.Subscribe(new List<string>()
      {
        (topic + _configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX")).Trim()
      });
    }

    public async Task<Task> StartProcessingAsync(CancellationTokenSource token)
    {
      _log.LogDebug($"{nameof(StartProcessingAsync)} Topic {_topicName}: StartProcessingAsync");
      _stopToken = token;

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
              _log.LogError(ex, $"{nameof(StartProcessingAsync)} Topic {_topicName}: Unhandled exception occured: ");
            }
          }
        }, TaskCreationOptions.LongRunning)
        .ContinueWith((o) =>
        {
          _log.LogWarning($"{nameof(StartProcessingAsync)} Topic {_topicName}: StartProcessingAsync has been cancelled");
          if (o.Exception != null)
          {
            _log.LogCritical(o.Exception, $"{nameof(StartProcessingAsync)} Topic {_topicName}: Exception: ");
          }

          return Task.FromResult(1);
        });
    }

    /// <summary>
    ///  for tests
    /// </summary>
    public void StartProcessingSync()
    {
      if (_log.IsTraceEnabled())
        _log.LogTrace($"{nameof(StartProcessingSync)} Topic {_topicName}: Processing synchronous poll");
      int i = ProcessMessage().Result;
    }

    private async Task<int> ProcessMessage()
    {
      if (_log.IsTraceEnabled())
        _log.LogTrace($"{nameof(ProcessMessage)} Topic {_topicName}: SyncPolling with {_requestTime} timeout");
      var messages = _kafkaDriver.Consume(TimeSpan.FromMilliseconds(_requestTime));

      if (messages.message == Error.NO_ERROR)
      {
        await ProcessAllMessages(messages);
      }

      return 0;
    }

    public void SubscribeObserverConsumer()
    {
      _log.LogDebug(
        $"{nameof(SubscribeObserverConsumer)} Topic {_topicName}: Subscribing to consumer for message processing");
      _kafkaDriver.SubscribeConsumer(OnMessagesArrived, null);
    }

    private int OnMessagesArrived(Message messages)
    {
      ProcessAllMessages(messages).WaitAndUnwrapException();
      return 0;
    }

    private async Task ProcessAllMessages(Message messages)
    {
      bool success = true;
      foreach (var message in messages.payload)
      {
        try
        {
          await ProcessSingleMessage(message);
        }
        catch (Exception ex)
        {
          _log.LogError(ex, $"{nameof(ProcessAllMessages)} Topic {_topicName}: An unexpected exception occured: ");
          success = false;
        }
      }

      if (success)
      {
        if (_log.IsTraceEnabled())
          _log.LogTrace($"{nameof(ProcessAllMessages)} Topic {_topicName}: Kafka Commiting Partition {messages.partition} Offset: {messages.offset}");
        _kafkaDriver.Commit();
      }
    }

    private async Task ProcessSingleMessage(byte[] message)
    {
      string bytesAsString = Encoding.UTF8.GetString(message, 0, message.Length);
      _log.LogDebug(
        $"{nameof(ProcessSingleMessage)} Topic {_topicName}: messageType: {typeof(T)} content: {bytesAsString}");

      var converter = _messageResolver.GetConverter<T>();
      if (converter == null)
      {
        _log.LogError($"{nameof(ProcessSingleMessage)} Topic {_topicName}: unsupported topic");
      }
      else
      {
        var deserializedObject = JsonConvert.DeserializeObject<T>(bytesAsString, converter);
        // we don't support all message within a kafka type
        if (deserializedObject != null)
        {
          _log.LogDebug(
            $"{nameof(ProcessSingleMessage)} Topic {_topicName}: Saving data type {deserializedObject.GetType()}");
          await _dbRepositoryFactory.GetRepository<T>().StoreEvent(deserializedObject);
        }
      }
    }

    public void StopProcessing()
    {
      _stopToken.Cancel();
    }

    public void Dispose()
    {
      _kafkaDriver?.Dispose();
    }

  }
}
