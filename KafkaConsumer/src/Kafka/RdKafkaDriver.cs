
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
    Consumer rdConsumer = null;
    Producer rdProducer = null;

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

    public bool IsInitializedProducer { get; private set; } = false;
    public bool IsInitializedConsumer { get; private set; } = false;

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

    public Message Consume(TimeSpan timeout)
    {
      var payloads = new List<byte[]>();

      Confluent.Kafka.Message result = null;
      int protectionCounter = 0;
      
      while (payloads.Count < batchSize && protectionCounter < 10) //arbitary number here for the perfomance testing
      {
        log?.LogTrace($"Polling with {timeout.Milliseconds} ms and retries {protectionCounter}");
        rdConsumer.Consume(out result, timeout);
        if (result == null)
        {
          protectionCounter++;
          continue;
        }
        log?.LogTrace($"Polled with the result {result.Error.Code}");
        if (!result.Error.HasError)
        {
          payloads.Add(result.Value);
        }
        protectionCounter++;
      }

      return result != null
        ? new Message(payloads, Error.NO_ERROR, result.Offset, result.Partition)
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

      Console.WriteLine("KAFKA_GROUP_NAME:" + ConsumerGroup);
      Console.WriteLine("KAFKA_AUTO_COMMIT:" + EnableAutoCommit);
      Console.WriteLine("KAFKA_OFFSET:" + OffsetReset);
      Console.WriteLine("KAFKA_URI:" + Uri);
      Console.WriteLine("KAFKA_PORT:" + Port);

      consumerConfig = new Dictionary<string, object>
      {
        {"bootstrap.servers", Uri},
        {"enable.auto.commit", EnableAutoCommit},
        {"group.id", ConsumerGroup},
        {"session.timeout.ms", "179000"},
        {"request.timeout.ms", "180000"},
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

      producerConfig = new Dictionary<string, object>
      {
        {"bootstrap.servers", Uri},
        {"session.timeout.ms", "10000"},
        {"retries", "3"},
        {"batch.size", "1048576"},
        {"linger.ms", "20"},
        {"acks", "all"},
        {"block.on.buffer.full", "true"}
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
      }
      rdProducer?.Dispose();
      IsInitializedProducer = false;
      IsInitializedConsumer = false;

    }
  }
}
