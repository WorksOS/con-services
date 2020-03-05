using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VSS.MasterData.WebAPI.Data.Confluent
{
	public class ConfluentPublisher : Publisher
	{
		private readonly IConfiguration configuration;
		private static IProducer<string, string> confluentProducer;
		private NameValueCollection producerCollection;
		private readonly ILogger log;

		public ConfluentPublisher(IConfiguration configuration, ILogger log, NameValueCollection producerCollection = null) : base(log)
		{
			this.log = log;
			this.producerCollection = producerCollection;
			this.configuration = configuration;
		}

		protected sealed override void InitProducer()
		{
			object newObj=new object();
			lock (newObj)
			{
				confluentProducer = new ProducerBuilder<string, string>(BuildConfig()).Build();
			}
		}

		protected override void Send(string topic, IEnumerable<KeyValuePair<string, string>> messagesToSendWithKeys)
		{
			if (confluentProducer is null)
				InitProducer();
			var sendResults = (from messageToSend in messagesToSendWithKeys
							   select new Message<string, string>()
							   {
								   Key = messageToSend.Key,
								   Value = messageToSend.Value,
							   } into msgToSend
							   let correctPartition = Partitioner.GetPartitionNumber(msgToSend.Key, 6)
							   let tp = new TopicPartition(topic, correctPartition)
							   select confluentProducer.ProduceAsync(tp, msgToSend) into task
							   where task != null
							   select new KeyValuePair<string, Task<DeliveryResult<string, string>>>(topic, task)
							   ).ToList();
			//var sendResults = (from messageToSend in messagesToSendWithKeys select new Message<string, string>() { Key = messageToSend.Key, Value = messageToSend.Value, } into msgToSend let correctPartition = Partitioner.GetPartitionNumber(msgToSend.Key, 6) let tp = new TopicPartition(topic, correctPartition) select confluentProducer.ProduceAsync(tp, msgToSend) into task where task != null select new KeyValuePair<string, Task<DeliveryResult<string, string>>>(topic, task)).ToList();
			// Check all the messages are published correctly. 
			BlockUntilSendComplete(sendResults);
		}


		private void BlockUntilSendComplete(List<KeyValuePair<string, Task<DeliveryResult<string, string>>>> producingTasks)
		{
			foreach (KeyValuePair<string, Task<DeliveryResult<string, string>>> topicAndTask in producingTasks)
			{
				string topic = topicAndTask.Key;
				Task<DeliveryResult<string, string>> deliveryReport = topicAndTask.Value;

				try
				{
					deliveryReport.Wait();
				}
				catch (Exception e)
				{
					if (e.Message.Contains("Broker: Unknown topic or partition"))
					{
						log.LogError($"It is most likely that this code tried to produce to {topic} topic and that topic does not exist"); ;
					}

					log.LogError(e, $"waiting for producing task for {topic} topic threw an exception. Task exception: {deliveryReport?.Exception?.Message}.");
				}

				if (deliveryReport.IsCompleted == false || (deliveryReport.IsFaulted || deliveryReport.IsCanceled) && deliveryReport.Exception != null)
				{
					log.LogError($"Publishing to kafka failed with error {deliveryReport.Exception }");
					throw new Exception($"Publishing to kafka failed with error: {deliveryReport.Exception?.Message}");
				}
			}
		}

		public override void Dispose()
		{
			confluentProducer?.Flush(new TimeSpan(0, 0, 0, 1));
			confluentProducer?.Dispose();
			confluentProducer = null;
		}

		private List<KeyValuePair<string, string>> BuildConfig()
		{
			var notSupportedConfigKeys = new List<string> { "batch.size", "buffer.memory", "max.block.ms", "value.serializer", "value.deserializer", "key.deserializer", "key.serializer", "block.on.buffer.full" };
			var producerConfigKeyValuePairs = new List<KeyValuePair<string, string>>();

			if (producerCollection == null)
			{
				producerCollection = ConfigUtility.ConvertToNameValueCollection(configuration, "producerSettings");
			}

			if (!producerCollection.AllKeys.Contains("bootstrap.servers"))
			{
				throw new Exception("bootstrap.servers is not in the producer setting.");
			}

			foreach (string key in producerCollection.AllKeys)
			{
				if (notSupportedConfigKeys.Contains(key))
				{
					continue;
				}
				producerConfigKeyValuePairs.Add(new KeyValuePair<string, string>(key, producerCollection[key]));
			}
			return producerConfigKeyValuePairs;
		}
	}
}
