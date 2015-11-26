using Misakai.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSP.MasterData.Common.KafkaWrapper.Interfaces;
using VSP.MasterData.Common.KafkaWrapper.Models;

namespace VSP.MasterData.Common.KafkaWrapper
{
  public class ConsumerWrapper : IConsumerWrapper
  {
    private readonly IEventAggregator _eventAggregator;
    private readonly KafkaConsumerParams _kafkaConsumerParams;

    public ConsumerWrapper(IEventAggregator eventAggregator, KafkaConsumerParams kafkaConsumerParams)
    {
      _eventAggregator = eventAggregator;
      _kafkaConsumerParams = kafkaConsumerParams;
    }

		public void Consume(bool fetchFromBeginning = false, bool fetchFromTail = false)
		{
			var options = new KafkaOptions(_kafkaConsumerParams.ServerUris.Select(x => new Uri(x)).ToArray());
			//"http://hadoopmaster.vssint.trimblepaas.com:6667"
			var consumerOptions = new ConsumerOptions(_kafkaConsumerParams.Topic, new BrokerRouter(options));

			if (!options.KafkaServerEndpoints.Any())
				return; //todo log warning
			List<IKafkaConnection> connections = options.KafkaServerEndpoints.
				Select(uri => options.KafkaConnectionFactory.Create(uri, options.ResponseTimeoutMs, options.Log)).ToList();

			OffsetPosition[] offsets;


			if (fetchFromBeginning)
				offsets = new OffsetPosition[] { };
			else if (fetchFromTail)
			{
				var cons = new Consumer(consumerOptions, new OffsetPosition[] { });
				var positions = cons.GetTopicOffsetAsync(_kafkaConsumerParams.Topic);
				positions.Wait();
				offsets = positions.Result
						.Select(p => new OffsetPosition(p.PartitionId, p.Offsets.Last()))
						.ToArray();
			}
			else
				offsets = _kafkaConsumerParams.Position == null
					? GetOffsetPositionsForTopic(consumerOptions, connections.First(), _kafkaConsumerParams.ConsumerGroup)
					: new[] { new OffsetPosition(_kafkaConsumerParams.Position.PartitionId, _kafkaConsumerParams.Position.OffSetId) };

			var consumer = new Consumer(consumerOptions, offsets);

			foreach (Message message in consumer.Consume())
			{
				if (message.Value == null)
					continue;
				var msg = new KafkaMessage
				{
					Key = message.Key == null ? null : Encoding.Default.GetString(message.Key),
					Value = Encoding.Default.GetString(message.Value),
					OffSet = message.Offset,
					PartitionId = message.PartitionId
				};

				try
				{
					_eventAggregator.ProcessMessage(msg);
				}
				catch
				{
					break;
				}
				StoreOffset(message.PartitionId, message.Offset, connections.First());
			}
		}

    private OffsetPosition[] GetOffsetPositionsForTopic(ConsumerOptions options, IKafkaConnection conn,
      string consumerGroup)
    {
      var client = new Producer(options.Router);

      List<OffsetResponse> topicOffsets = client.GetTopicOffsetAsync(options.Topic).Result;
      List<OffsetFetch> offsetFetches = topicOffsets.GroupBy(x => x.PartitionId)
        .Select(x => new OffsetFetch {PartitionId = x.Key, Topic = options.Topic}).ToList();

      var fetch = new OffsetFetchRequest
      {
        ConsumerGroup = consumerGroup,
        Topics = offsetFetches
      };
      List<OffsetFetchResponse> fetchResponse = conn.SendAsync(fetch).Result;
      OffsetPosition[] offsets = fetchResponse.Select(x => new OffsetPosition(x.PartitionId, x.Offset)).ToArray();

      return offsets;
    }

    private void StoreOffset(int partitionId, long offset, IKafkaConnection conn)
    {
      var commit = new OffsetCommitRequest
      {
        ConsumerGroup = _kafkaConsumerParams.ConsumerGroup,
        OffsetCommits = new List<OffsetCommit>
        {
          new OffsetCommit
          {
            PartitionId = partitionId,
            Topic = _kafkaConsumerParams.Topic,
            Offset = offset,
            Metadata = null
          }
        }
      };

      conn.SendAsync(commit);
    }
  }

  public interface IConsumerWrapper
  {
		void Consume(bool fetchFromBeginning = false, bool fetchFromTail = false);
  }
}