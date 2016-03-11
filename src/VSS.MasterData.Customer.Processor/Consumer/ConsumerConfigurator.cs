using System;
using VSS.Kafka.DotNetClient;
using VSS.Kafka.DotNetClient.Interfaces;
using VSS.Kafka.DotNetClient.Model;
using VSS.Customer.Processor.Interfaces;

namespace VSS.Customer.Processor.Consumer
{
	public class ConsumerConfigurator : IConsumerConfigurator
	{

		public string BaseUrl { get; set; }
		public string TopicName { get; set; }
		string GroupName { get; set; }
		string InstanceId { get; set; }
		int MaxBytes { get; set; }
		bool ContinueOnParseError { get; set; }

		private ConsumerInstance _cInstance;
		public ConsumerInstance ConsumerInstance { get { return _cInstance; } }

		private CreateConsumerRequest _cRequest;
		public CreateConsumerRequest ConsumerRequest { get { return _cRequest; } }

		private IRestProxySettings _settings;
		public IRestProxySettings RestProxySettings { get { return _settings; } }

		public ConsumerConfigurator(string baseUrl, string topicName, string groupName, string instanceId, int maxbytes = -1, bool continueOnParseError = true)
		{
			BaseUrl = baseUrl;
			TopicName = topicName;
			GroupName = groupName;
			InstanceId = instanceId;
			MaxBytes = maxbytes;
			ContinueOnParseError = continueOnParseError;
			Configure();
		}
		private void Configure()
		{
      _settings = new DefaultRestProxySettings();

			_cInstance = new ConsumerInstance {InstanceId = InstanceId};

			_cRequest = new CreateConsumerRequest
			{
				InstanceId = InstanceId,
				MessageFormat = MessageFormat.Binary,
				GroupName = GroupName,
				Topic = TopicName
			};
		
		}

		public IRestProxySettings GetRestProxySettings()
		{
			return RestProxySettings;
		}

		public CreateConsumerRequest GetConsumerRequest()
		{
			return ConsumerRequest;
		}
	}



}
