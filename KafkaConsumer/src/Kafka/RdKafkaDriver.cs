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
    private Consumer rdConsumer;
    private Producer rdProducer;

    private readonly Object syncPollObject = new object();
    private Dictionary<string, object> consumerConfig;
    private Dictionary<string, object> producerConfig;
    private int batchSize;
    private ILogger<IKafka> log;
    
    public string ConsumerGroup { get; set; }

    public string Uri { get; set; }

    public string OffsetReset { get; set; }

    public bool EnableAutoCommit { get; set; }

    public int Port { get; set; }

    public bool IsInitializedProducer { get; private set; }
    public bool IsInitializedConsumer { get; private set; }

    private Func<Message, int> onMessagesArrivedAction;
    private Action onCompletedAction;
    private Message messageQueue = new Message();

    public async Task<CommittedOffsets> Commit()
    {
     // Console.WriteLine($"Comitting offsets");
      var comittedOffsets = rdConsumer.CommitAsync().ContinueWith(o =>
      {
        log?.LogTrace(
          $"Committed number of offsets {o.Result.Offsets.Count()} with result {o.Result.Error.Reason} {o.Result.Error.Code}");
        return o.Result;
      }).Result;
      return comittedOffsets;
    }

    public void SubscribeConsumer(Func<Message,int> onMessagesArrived, Action onCompleted)
    {
      onMessagesArrivedAction = onMessagesArrived;
      onCompletedAction = onCompleted;
      rdConsumer.OnMessage += RdConsumer_OnMessage;
    }

    private void RdConsumer_OnMessage(object sender, Confluent.Kafka.Message e)
    {
      if (e != null)
      {
        log?.LogTrace($"Polled with the result {e.Error.Code}");

        if (!e.Error.HasError)
        {
          messageQueue.AddPayload(e.Value);
        }
        //Ignore batching here for some time
       // if (messageQueue.payload.Count() > batchSize)
        {
          onMessagesArrivedAction(messageQueue);
          messageQueue.ClearPayloads();
          onCompletedAction?.Invoke();
        }
      }
    }

    public Message Consume(TimeSpan timeout)
    {
      var payloads = new List<byte[]>();

      Confluent.Kafka.Message lastValidResult = null;

      int protectionCounter = 0;

      while (payloads.Count < batchSize && protectionCounter < 10) //arbitary number here for the perfomance testing
      {
        log?.LogTrace($"Polling with retries {protectionCounter}");
        log?.LogTrace($"Consumer is subscribed to {rdConsumer.Subscription.Aggregate((i, j) => i + j)}");
        rdConsumer.Consume(out var result, timeout);
        if (result == null)
        {
          protectionCounter++;
          continue;
        }
        log?.LogTrace($"Polled with the result {result.Error.Code}");
        if (!result.Error.HasError)
        {
          payloads.Add(result.Value);
          lastValidResult = result;
        }
        else
          protectionCounter++;
      }

      log?.LogTrace(
        $"Returning {payloads.Count} records with offset {lastValidResult?.Offset ?? -1} and partition {lastValidResult?.Partition ?? -1}");
      return payloads.Count > 0
        ? new Message(payloads, Error.NO_ERROR, lastValidResult?.Offset ?? -1, lastValidResult?.Partition ?? -1)
        : new Message(payloads, Error.NO_DATA);
    }


    public void InitConsumer(IConfigurationStore configurationStore, string groupName = null, ILogger<IKafka> logger = null)
    {
      this.log = logger;
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

      log?.LogTrace($"InitConsumer: KAFKA_GROUP_NAME:{ConsumerGroup}  KAFKA_AUTO_COMMIT: {EnableAutoCommit}  KAFKA_OFFSET: {OffsetReset}  KAFKA_URI: {Uri}  KAFKA_PORT: {Port} KAFKA_CONSUMER_SESSION_TIMEOUT:{sessionTimeout}  KAFKA_REQUEST_TIMEOUT: {requestTimeout}");
      
      consumerConfig = new Dictionary<string, object>
      {
        {"bootstrap.servers", Uri},
        {"enable.auto.commit", EnableAutoCommit},
        {"group.id", ConsumerGroup},
        {"session.timeout.ms", sessionTimeout},
        {"request.timeout.ms", requestTimeout},
        {"auto.offset.reset", OffsetReset},
      };

      IsInitializedConsumer = true;
    }

    public void Subscribe(List<string> topics)
    {
      rdConsumer = new Consumer(consumerConfig);
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

      producerConfig = new Dictionary<string, object>
      {
        {"bootstrap.servers", Uri},
        {"session.timeout.ms", sessionTimeout},
        {"retries", "3"},
        {"linger.ms", "20"},
        {"acks", "all"}
      };

      //socket.blocking.max.ms=1
      rdProducer = new Producer(producerConfig);
      IsInitializedProducer = true;
    }

    public void Send(string topic, IEnumerable<KeyValuePair<string, string>> messagesToSendWithKeys)
    {
      List<Task> tasks = new List<Task>();
      foreach (var messagesToSendWithKey in messagesToSendWithKeys)
      {
        byte[] data = Encoding.UTF8.GetBytes(messagesToSendWithKey.Value);
        byte[] key = Encoding.UTF8.GetBytes(messagesToSendWithKey.Key);
        tasks.Add(rdProducer.ProduceAsync(topic, key, data));
      }
      Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(10));
    }

    public async Task Send(string topic, KeyValuePair<string, string> messageToSendWithKey)
    {
      byte[] data = Encoding.UTF8.GetBytes(messageToSendWithKey.Value);
      byte[] key = Encoding.UTF8.GetBytes(messageToSendWithKey.Key);
      await rdProducer.ProduceAsync(topic, key, data);
    }

    public void Send(IEnumerable<KeyValuePair<string, KeyValuePair<string, string>>> topicMessagesToSendWithKeys)
    {
      List<Task> tasks = new List<Task>();
      foreach (var topicMessagesToSendWithKey in topicMessagesToSendWithKeys)
      {
        byte[] data = Encoding.UTF8.GetBytes(topicMessagesToSendWithKey.Value.Value);
        byte[] key = Encoding.UTF8.GetBytes(topicMessagesToSendWithKey.Value.Key);
        tasks.Add(rdProducer.ProduceAsync(topicMessagesToSendWithKey.Key, key, data));
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
