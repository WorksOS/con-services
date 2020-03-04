using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonModel.AssetSettings
{
	public class Configurations
	{
		public ConnectionStrings ConnectionStrings { get; set; }
		public ApplicationSettings ApplicationSettings { get; set; }
		public KafkaSettings KafkaSettings { get; set; }
	}

	public class ConnectionStrings
	{
		public string ReadMySqlDatabase { get; set; }
		public string WriteMySqlDatabase { get; set; }
	}

	public class ApplicationSettings
	{
		public string ClearOffPendingValueForDeviceTypes { get; set; }
		public int ClearOffPendingValueGreaterThanNoOfDays { get; set; }
		public int? DefaultPageNumber { get; set; }
		public int? DefaultPageSize { get; set; }
	}

	public class KafkaSettings
	{
		public string Brokers { get; set; }
		public PublisherTopics PublisherTopics { get; set; }
	}

	public class PublisherTopics
	{
		public string AssetTopicNames { get; set; }
		public string WorkDefinitionTopicNames { get; set; }
		public string AssetOwnerTopicName { get; set; }
		public string AssetEcmInfoTopicName { get; set; }
		public string AssetSettingsTopicName { get; set; }
		public string AssetWeeklySettingsTopicName { get; set; }
		public string UserAssetSettingsTopicName { get; set; }
		public string UserAssetWeeklySettingsTopicName { get; set; }
	}
}
