using KafkaModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.MasterData.WebAPI.Data.Confluent
{
	public abstract class Publisher : IPublisher
	{
		protected int WaitTimeOut = 60000;
		private readonly ILogger log;

		protected Publisher(ILogger log)
		{
			this.log = log;
		}

		public void Publish(KafkaMessage kafkaMessage)
		{
			Publish(new List<KafkaMessage> { kafkaMessage });
		}

		public void Publish(List<KafkaMessage> kafkaMessages)
		{
			foreach (var messagesPerTopic in kafkaMessages.GroupBy(x => x.Topic))
			{
				Publish(messagesPerTopic.First().Topic, messagesPerTopic);
			}
		}

		private KeyValuePair<string, string> GetKeyValuePair(KafkaMessage message)
		{
			return new KeyValuePair<string, string>(message.Key, message.Message is string ? message.Message as string : JsonConvert.SerializeObject(message.Message));
		}

		private void Publish(string topic, IEnumerable<KafkaMessage> messages)
		{
			var keyMessages = new List<KeyValuePair<string, string>>();
			foreach (var message in messages)
			{
				if (message.Message is IEnumerable<object>)
				{
					throw new Exception("The List of KafkaMessage should not contains List of Message. Please split the message and send it as List<KafkaMessage>");
				}

				keyMessages.Add(this.GetKeyValuePair(message));
			}

			if (!Task.Run(() => Send(topic, keyMessages)).Wait(WaitTimeOut))
			{
				throw new Exception($"Publisher cannot complete the work on the specific time {WaitTimeOut} ms");
			}
		}


		public void RetryPublish(KafkaMessage kafkaMessage)
		{
			Publish(kafkaMessage);
		}

		public void RetryPublish(List<KafkaMessage> kafkaMessages)
		{
			Publish(kafkaMessages);
		}

		protected abstract void InitProducer();

		protected abstract void Send(string topic, IEnumerable<KeyValuePair<string, string>> messagesToSendWithKeys);
		public abstract void Dispose();
	}
}
