using System.Collections.Generic;
using KafkaModel;

namespace VSS.MasterData.WebAPI.Data.Confluent
{
	public interface IPublisher
	{
		void Publish(KafkaMessage kafkaMessage);
		void Publish(List<KafkaMessage> kafkaMessages);
		void RetryPublish(KafkaMessage kafkaMessage);
		void RetryPublish(List<KafkaMessage> kafkaMessages);
		void Dispose();
	}
}
