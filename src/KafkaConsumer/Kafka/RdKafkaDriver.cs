using RdKafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.UnifiedProductivity.Service.Utils;
using VSS.UnifiedProductivity.Service.Interfaces;

namespace VSS.UnifiedProductivity.Service.Utils.Kafka
{
  public class RdKafkaDriver : IKafka
  {
    Consumer rdConsumer = null;
    private readonly Object syncPollObject = new object();
    private Config config;

    public string ConsumerGroup
    {
      get;
      set;
    }

    public string Uri
    {
      get;
      set;
    }

    public string OffsetReset
    {
      get;
      set;
    }

    public bool EnableAutoCommit
    {
      get;
      set;
    }
        
    public int Port
    {
      get;
      set;
    }

    public void Commit()
    {
      rdConsumer.Commit();
    }

    public Interfaces.Message Consume(TimeSpan timeout)
    {
      lock (syncPollObject)
      {
        var result = rdConsumer.Consume(timeout);
        if (result.HasValue)
          if (result.Value.Error == ErrorCode.NO_ERROR)
            return new Interfaces.Message(new List<byte[]>() { result.Value.Message.Payload }, Interfaces.Error.NO_ERROR);
          else
            return new Interfaces.Message(null, (Interfaces.Error)(int)result.Value.Error);
        else
          return new Interfaces.Message(null, Interfaces.Error.NO_DATA);
      }
    }

    public void InitConsumer(IConfigurationStore configurationStore, string groupName = null)
    {
      ConsumerGroup = groupName == null ? configurationStore.GetValueString("KAFKA_GROUP_NAME") : groupName;
      EnableAutoCommit = configurationStore.GetValueBool("KAFKA_AUTO_COMMIT").Value;
      OffsetReset = configurationStore.GetValueString("KAFKA_OFFSET");
      Uri = configurationStore.GetValueString("KAFKA_URI");
      Port = configurationStore.GetValueInt("KAFKA_PORT");

      var topicConfig = new TopicConfig();
      topicConfig["auto.offset.reset"] = OffsetReset;

      config = new Config()
      {
        GroupId = ConsumerGroup,
        EnableAutoCommit = EnableAutoCommit,
        DefaultTopicConfig = topicConfig
      };
    }

    public void Subscribe(List<string> topics)
    {
      rdConsumer = new Consumer(config, Uri);
      rdConsumer.Subscribe(topics);
    }
    public void Dispose()
    {
      lock (syncPollObject)
      {
          rdConsumer?.Unsubscribe();
          rdConsumer?.Dispose();
      }
    }
  }
}
