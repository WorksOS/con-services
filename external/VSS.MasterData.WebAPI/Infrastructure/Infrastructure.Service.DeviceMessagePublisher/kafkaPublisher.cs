using CommonModel.DeviceSettings;
using Infrastructure.Service.DeviceMessagePublisher.Interfaces;
using KafkaModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using VSS.MasterData.WebAPI.Transactions;
using VSS.VisionLink.Interfaces.Events.DeviceConfig;

namespace Infrastructure.Service.DeviceMessagePublisher
{
	public class DeviceMessageKafkaPublisher : IDeviceMessageKafkaPublisher
    {
		private const string ObjectEmbedder = "{{ \"{0}\" : {1} }}";
		private readonly Dictionary<string,string> _topicNames;
		private readonly ITransactions _transaction;

		public DeviceMessageKafkaPublisher(Configurations configuration, ITransactions transaction)
        {
			_transaction = transaction;
			if (String.IsNullOrWhiteSpace(configuration.KafkaSettings.PublisherTopics.PL_TopicName) || 
				String.IsNullOrWhiteSpace(configuration.KafkaSettings.PublisherTopics.MTS_TopicName) || 
				String.IsNullOrWhiteSpace(configuration.KafkaSettings.PublisherTopics.DataOut_TopicName) || 
				String.IsNullOrWhiteSpace(configuration.KafkaSettings.PublisherTopics.HoursMeterOffset_TopicName) || 
				String.IsNullOrWhiteSpace(configuration.KafkaSettings.PublisherTopics.OdometerOffset_TopicName) || 
				String.IsNullOrWhiteSpace(configuration.KafkaSettings.PublisherTopics.DeviceConfiguredMessage_TopicName) || 
				String.IsNullOrWhiteSpace(configuration.KafkaSettings.PublisherTopics.MTSServerSideRunTimeCalibration_TopicName))
				throw new ArgumentException("Topic Names are Empty!!");
			_topicNames = new Dictionary<string, string> 
			{
				{"PL", configuration.KafkaSettings.PublisherTopics.PL_TopicName },
				{"MTS", configuration.KafkaSettings.PublisherTopics.MTS_TopicName },
				{"DataOut", configuration.KafkaSettings.PublisherTopics.DataOut_TopicName },
				{"RuntimeHoursOffset", configuration.KafkaSettings.PublisherTopics.HoursMeterOffset_TopicName },
				{"OdometerOffset", configuration.KafkaSettings.PublisherTopics.OdometerOffset_TopicName },
				{"DeviceConfig", configuration.KafkaSettings.PublisherTopics.DeviceConfiguredMessage_TopicName },
				{"MTSServerSideRunTimeCalibration", configuration.KafkaSettings.PublisherTopics.MTSServerSideRunTimeCalibration_TopicName },
			};
		}
        public bool PublishMessage(string kafkaKey, IEnumerable<object> kafkaObjects, string deviceFamily)
        {
            var isPublished = false;

			if (!String.IsNullOrEmpty(deviceFamily))
			{
				if (!_topicNames.ContainsKey(deviceFamily))
					return isPublished;

				var payloadMessages = kafkaObjects.Select(x => new KafkaMessage
				{
					Key = kafkaKey,
					Message = string.Format(ObjectEmbedder, x.GetType().Name, JsonConvert.SerializeObject(x)),
					Topic = _topicNames[deviceFamily]
				}).ToList();

				_transaction.Publish(payloadMessages);
				isPublished = true;
			}
			else
			{
				foreach (var kafkaObject in kafkaObjects.GroupBy(x => x.GetType()))
				{
					if (kafkaObject.Any())
					{
						var topic = _topicNames[kafkaObject.FirstOrDefault().GetType().Name];

						var payloadMessages = kafkaObject.Select(x => new KafkaMessage
						{
							Key = kafkaKey,
							Message = JsonConvert.SerializeObject(x),
							Topic = topic
						}).ToList();

						_transaction.Publish(payloadMessages);
						isPublished = true;
					}
					else
					{
						isPublished = false;
					}
				}
			}
            return isPublished;
        }

		public bool PublishMessage(IEnumerable<dynamic> kafkaObjects, string deviceFamily)
		{
			var isPublished = false;
			try
			{
				if (!String.IsNullOrEmpty(deviceFamily))
				{
					if (!_topicNames.ContainsKey(deviceFamily))
						return isPublished;



					var payloadMessages = kafkaObjects.Select(x =>
					{
						dynamic message = new ExpandoObject();
						(message as IDictionary<string, object>).Add(x.GetType().Name, x);
						return new KafkaMessage
						{
							Key = x.Context.DeviceUid.ToString(),
							Message = message,
							Topic = _topicNames[deviceFamily]
						};
					}).ToList();
					
					_transaction.Publish(payloadMessages);
					isPublished = true;

				}
			}
			catch(Exception ex)
			{
				throw ex;
			}
			return isPublished;
		}

		public bool PublishDeviceConfiguredMessage(List<DeviceConfig> deviceConfiguredMessages)
        {
            var isPublished = false;
			try
			{
				var typeName = deviceConfiguredMessages.First().GetType().Name;

				if (!_topicNames.ContainsKey(typeName))
					return isPublished;

				var payloadMessages = deviceConfiguredMessages.Select(x => new KafkaMessage
				{
					Key = x.DeviceUID.ToString(),
					Message = string.Format(ObjectEmbedder, x.GetType().Name, JsonConvert.SerializeObject(x) ),
					Topic = _topicNames[typeName]
				}).ToList();

				_transaction.Publish(payloadMessages);
				isPublished = true;
			}
			catch (Exception ex)
			{
				throw ex;
			}
            return isPublished;
        }
	}
}
