using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonModel.DeviceSettings
{
	public class Configurations
	{
		public ConnectionStrings ConnectionString { get; set; }
		public ApplicationSettings AppSettings { get; set; }
		public KafkaSettings KafkaSettings { get; set; }
	}

	public class ConnectionStrings
	{
		public string MasterData { get; set; }
	}

	public class ApplicationSettings
	{
		public string ClearOffPendingValueForDeviceTypes { get; set; }
		public int ClearOffPendingValueGreaterThanNoOfDays { get; set; }

		public string ServerSideRunTimeCalibrationDeviceTypes { get; set; }
		public string RemoveServerSideRunTimeCalibrationDeviceTypes { get; set; } //Remove if not needed
		public string NewlySupportedRunTimeCalibrationDeviceTypes { get; set; } 

		public string DeviceConfigSendAllSwitchesDeviceFamilyLists { get; set; }
		public int DeviceConfigParameterAttributeCacheTimeOut { get; set; }
		public string DeviceConfigParameterGroupsNonMandatoryLists { get; set; }
		public string DeviceConfigParametersNonMandatoryLists { get; set; } //Remove if not needed
		public string DeviceConfigAttributesNonMandatoryLists { get; set; } //Remove if not needed

		public int DataOut_PingInterval { get; set; }
		public int MTS_PingInterval { get; set; }
		public int PL_PingInterval { get; set; }

		public int TimeDurationToHitDataBaseInMins { get; set; } //Remove if not needed
	}

	public class KafkaSettings
	{
		public string Brokers { get; set; }
		public PublisherTopics PublisherTopics { get; set; }
	}

	public class PublisherTopics
	{
		public string PL_TopicName { get; set; }
		public string MTS_TopicName { get; set; }
		public string DataOut_TopicName { get; set; }
		public string HoursMeterOffset_TopicName { get; set; }
		public string OdometerOffset_TopicName { get; set; }
		public string DeviceConfiguredMessage_TopicName { get; set; }
		public string MTSServerSideRunTimeCalibration_TopicName { get; set; }
	}
}
