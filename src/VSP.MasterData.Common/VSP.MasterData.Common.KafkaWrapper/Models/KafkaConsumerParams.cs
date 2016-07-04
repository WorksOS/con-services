using System.Collections.Generic;

namespace VSP.MasterData.Common.KafkaWrapper.Models
{
  public class KafkaConsumerParams
  {
    public KafkaConsumerParams(string consumerGroup, string uri, string topic, KafkaTopicPosition position = null)
      : this(consumerGroup, new List<string> {uri}, topic, position)
    {
    }

    public KafkaConsumerParams(string consumerGroup, IList<string> uris, string topic,
      KafkaTopicPosition position = null)
    {
      ConsumerGroup = consumerGroup;
      ServerUris = uris;
      Topic = topic;
      Position = position;
    }

    public string ConsumerGroup { get; set; } //you need this for storing offsets
    public IList<string> ServerUris { get; set; }
    public string Topic { get; set; }
    public KafkaTopicPosition Position { get; set; }
  }

  public class KafkaTopicPosition
  {
    public long OffSetId { get; set; }
    public int PartitionId { get; set; }
  }
}