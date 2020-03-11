using System.Configuration;
using Microsoft.AspNetCore.Mvc;
using VSS.MasterData.WebAPI.ClientModel;

namespace VSS.MasterData.WebAPI.Device.Controllers.V1
{
	/// <summary>
	/// Events Controller to Collect the topic metadata of kafka events
	/// </summary>
	[Route("v1/EventStores")]
	[ApiController]
	public class EventStoresController : ControllerBase
	{
		#region Public Methods

		/// <summary>
		/// Get the topic metadata
		/// </summary>
		/// <returns></returns>
		[Route("")]
		[HttpGet]
		public Definitions GetTopicMetaData()
		{
			var definition = new Definitions { name = "VSSKafkaTopic" };

			var deviceTopic = GetDeviceTopic();

			definition.Topics = new[] { deviceTopic };

			return definition;
		}


		#endregion  Public Methods

		#region  Private Methods

		private static DefinitionsTopic GetDeviceTopic()
		{
			var deviceTopic = new DefinitionsTopic
			{
				DefaultKey = "DeviceUID",
				Name = ConfigurationManager.AppSettings["kafkaTopicNames"],
				URL = ConfigurationManager.AppSettings["restProxyBaseUrl"]
			};


			var deviceCreateEvent = new DefinitionsTopicEvent
			{
				Name = "CreateDeviceEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
				  "{\"CreateDeviceEvent\": \"{\"DeviceUID\": \"string\",\"DeviceSerialNumber\": \"string\",\"DeviceType\": \"string\",\"DeviceState\": \"string\",\"DeregisteredUTC\": \"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ModuleType\": \"string\",\"MainboardSoftwareVersion\": \"string\",\"RadioFirmwarePartNumber\": \"string\",\"GatewayFirmwarePartNumber\": \"string\",\"DataLinkType\": \"string\",\"FirmwarePartNumber\": \"string\",\"CellModemIMEI\": \"string\",\"DevicePartNumber\": \"string\",\"CellularFirmwarePartNumber\": \"string\",\"NetworkFirmwarePartNumber\": \"string\",\"SatelliteFirmwarePartNumber\": \"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var deviceUpdateEvent = new DefinitionsTopicEvent
			{
				Name = "UpdateDeviceEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
				  "{\"UpdateDeviceEvent\": \"{\"DeviceUID\": \"string\",\"DeviceSerialNumber\": \"string\",\"DeviceType\": \"string\",\"DeviceState\": \"string\",\"DeregisteredUTC\": \"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ModuleType\": \"string\",\"MainboardSoftwareVersion\": \"string\",\"RadioFirmwarePartNumber\": \"string\",\"GatewayFirmwarePartNumber\": \"string\",\"DataLinkType\": \"string\",\"FirmwarePartNumber\": \"string\",\"CellModemIMEI\": \"string\",\"DevicePartNumber\": \"string\",\"CellularFirmwarePartNumber\": \"string\",\"NetworkFirmwarePartNumber\": \"string\",\"SatelliteFirmwarePartNumber\": \"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var associateDeviceAssetEvent = new DefinitionsTopicEvent
			{
				Name = "AssociateDeviceAssetEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
				  "{\"AssociateDeviceAssetEvent\": \"{\"DeviceUID\": \"string\",\"AssetUID\": \"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var dissociateDeviceAssetEvent = new DefinitionsTopicEvent
			{
				Name = "DissociateDeviceAssetEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
				  "{\"DissociateDeviceAssetEvent\": \"{\"DeviceUID\": \"string\",\"AssetUID\": \"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var deviceTransferEvent = new DefinitionsTopicEvent
			{
				Name = "DeviceTransferEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
				  "{\"DeviceTransferEvent\": \"{\"DeviceUID\": \"string\",\"OldAssetUID\": \"string\",\"NewAssetUID\": \"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var deviceReplacementEvent = new DefinitionsTopicEvent
			{
				Name = "DeviceReplacementEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
				  "{\"DeviceReplacementEvent\": \"{\"OldDeviceUID\": \"string\",\"NewDeviceUID\": \"string\",\"AssetUID\": \"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			deviceTopic.Event = new[] { deviceCreateEvent, deviceUpdateEvent, associateDeviceAssetEvent, dissociateDeviceAssetEvent, deviceTransferEvent, deviceReplacementEvent };
			return deviceTopic;
		}

		#endregion  Private Methods
	}
}
