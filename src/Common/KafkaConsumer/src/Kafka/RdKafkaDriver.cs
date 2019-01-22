using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;

namespace VSS.KafkaConsumer.Kafka
{
  public class RdKafkaDriver : IKafka
  {
    private Consumer<Ignore, byte[]> rdConsumer;
    private Producer<byte[], byte[]> rdProducer;

    private readonly object syncPollObject = new object();
    private Dictionary<string, string> consumerConfig;
    private Dictionary<string, string> producerConfig;
    private int batchSize;
    private ILogger<IKafka> log;
    private ConsumeResult<Ignore, byte[]> lastValidResult;

    public string ConsumerGroup { get; set; }

    public string Uri { get; set; }

    public string OffsetReset { get; set; }

    public bool EnableAutoCommit { get; set; }

    public int Port { get; set; }

    public bool IsInitializedProducer { get; private set; }
    public bool IsInitializedConsumer { get; private set; }

    public TopicPartitionOffset Commit()
    {
      if (lastValidResult == null)
      {
        return null;
      }

      var committedOffsets = rdConsumer.Commit(lastValidResult);
      log?.LogTrace($"Committed number of offsets {lastValidResult}");

      return committedOffsets;
    }

    [Obsolete("Use Consume() instead")]
    public void SubscribeConsumer(Func<Message, int> onMessagesArrived, Action onCompleted)
    {
      throw new NotImplementedException();
    }

    public Message Consume(TimeSpan timeout)
    {
      var payloads = new List<byte[]>();
      int protectionCounter = 0;

      while (payloads.Count < batchSize && protectionCounter < 10) //arbitary number here for the perfomance testing
      {
        log?.LogTrace($"Polling with retries {protectionCounter}");
        log?.LogTrace($"Consumer is subscribed to {rdConsumer.Subscription[0]}");
        try
        {
          var result = rdConsumer.Consume(timeout);
          log?.LogTrace($"Polled with the OK result {result?.Headers} and value {result?.Value?.Length}");
          if (result?.Value != null)
          {
            payloads.Add(result.Value);
            lastValidResult = result;
          }
        }
        catch (ConsumeException e)
        {
          log?.LogTrace($"Polled with the result {e.Message}");
        }
        finally
        {
          protectionCounter++;
        }
      }

      log?.LogTrace(
        $"Returning {payloads.Count} records with offset {lastValidResult?.Offset ?? -1} and partition {lastValidResult?.Partition ?? -1}");
      return payloads.Count > 0
        ? new Message(payloads, Error.NO_ERROR, lastValidResult?.Offset ?? -1, lastValidResult?.Partition ?? -1)
        : new Message(payloads, Error.NO_DATA);
    }


    public void InitConsumer(IConfigurationStore configurationStore, string groupName = null, ILogger<IKafka> logger = null)
    {
      log = logger;
      ConsumerGroup = groupName ?? configurationStore.GetValueString("KAFKA_GROUP_NAME");
      EnableAutoCommit = configurationStore.GetValueBool("KAFKA_AUTO_COMMIT").Value;
      OffsetReset = configurationStore.GetValueString("KAFKA_OFFSET");
      Uri = configurationStore.GetValueString("KAFKA_URI");
      Port = configurationStore.GetValueInt("KAFKA_PORT");
      batchSize = configurationStore.GetValueInt("KAFKA_BATCH_SIZE");
      var sessionTimeout = 179000;
      if (configurationStore.GetValueInt("KAFKA_CONSUMER_SESSION_TIMEOUT") > -1)
        sessionTimeout = configurationStore.GetValueInt("KAFKA_CONSUMER_SESSION_TIMEOUT");
      var requestTimeout = 180000;
      if (configurationStore.GetValueInt("KAFKA_REQUEST_TIMEOUT") > -1)
        requestTimeout = configurationStore.GetValueInt("KAFKA_REQUEST_TIMEOUT");

      log?.LogTrace(
        $"InitConsumer: KAFKA_GROUP_NAME:{ConsumerGroup}  KAFKA_AUTO_COMMIT: {EnableAutoCommit}  KAFKA_OFFSET: {OffsetReset}  KAFKA_URI: {Uri}  KAFKA_PORT: {Port} KAFKA_CONSUMER_SESSION_TIMEOUT:{sessionTimeout}  KAFKA_REQUEST_TIMEOUT: {requestTimeout}");

      consumerConfig = new Dictionary<string, string>
      {
        {"bootstrap.servers", Uri},
        {"enable.auto.commit", EnableAutoCommit.ToString()},
        {"group.id", ConsumerGroup},
        {"session.timeout.ms", sessionTimeout.ToString()},
        {"request.timeout.ms", requestTimeout.ToString()},
        {"auto.offset.reset", OffsetReset},
      };

      IsInitializedConsumer = true;
    }

    public void Subscribe(List<string> topics)
    {
      rdConsumer = new Consumer<Ignore, byte[]>(consumerConfig.ToList());
      rdConsumer.Subscribe(topics);
    }

    public void InitProducer(IConfigurationStore configurationStore)
    {
      Uri = configurationStore.GetValueString("KAFKA_URI");
      Port = configurationStore.GetValueInt("KAFKA_PORT");
      var sessionTimeout = 10000;

      if (configurationStore.GetValueInt("KAFKA_PRODUCER_SESSION_TIMEOUT") > -1)
        sessionTimeout = configurationStore.GetValueInt("KAFKA_PRODUCER_SESSION_TIMEOUT");

      log?.LogTrace($"InitProducer: KAFKA_URI:{Uri}  KAFKA_PORT: {Port}  KAFKA_PRODUCER_SESSION_TIMEOUT: {sessionTimeout}");

      producerConfig = new Dictionary<string, string>
      {
        {"bootstrap.servers", Uri},
        {"session.timeout.ms", sessionTimeout.ToString()},
        {"retries", "3"},
        {"linger.ms", "20"},
        {"acks", "all"}
      };

      //socket.blocking.max.ms=1
      rdProducer = new Producer<byte[], byte[]>(producerConfig.ToList());
      IsInitializedProducer = true;
    }

    public void Send(string topic, IEnumerable<KeyValuePair<string, string>> messagesToSendWithKeys)
    {
      var tasks = new List<Task>();
      foreach (var messagesToSendWithKey in messagesToSendWithKeys)
      {
        byte[] data = Encoding.UTF8.GetBytes(messagesToSendWithKey.Value);
        byte[] key = Encoding.UTF8.GetBytes(messagesToSendWithKey.Key);
        tasks.Add(rdProducer.ProduceAsync(topic, new Message<byte[], byte[]> { Key = key, Value = data }));
      }

      Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(10));
    }

    public async Task Send(string topic, KeyValuePair<string, string> messageToSendWithKey)
    {
      byte[] data = Encoding.UTF8.GetBytes(messageToSendWithKey.Value);
      byte[] key = Encoding.UTF8.GetBytes(messageToSendWithKey.Key);
      await rdProducer.ProduceAsync(topic, new Message<byte[], byte[]> { Key = key, Value = data });
    }

    public void Send(IEnumerable<KeyValuePair<string, KeyValuePair<string, string>>> topicMessagesToSendWithKeys)
    {
      var tasks = new List<Task>();
      foreach (var topicMessagesToSendWithKey in topicMessagesToSendWithKeys)
      {
        byte[] data = Encoding.UTF8.GetBytes(topicMessagesToSendWithKey.Value.Value);
        byte[] key = Encoding.UTF8.GetBytes(topicMessagesToSendWithKey.Value.Key);
        tasks.Add(rdProducer.ProduceAsync(topicMessagesToSendWithKey.Key,
          new Message<byte[], byte[]> { Key = key, Value = data }));
      }

      Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(10));
    }

    public void Dispose()
    {
      lock (syncPollObject)
      {
        rdConsumer?.Unsubscribe();
        rdConsumer?.Dispose();
        rdConsumer = null;
      }

      rdProducer?.Dispose();
      rdProducer = null;
      IsInitializedProducer = false;
      IsInitializedConsumer = false;
    }
  }
}
