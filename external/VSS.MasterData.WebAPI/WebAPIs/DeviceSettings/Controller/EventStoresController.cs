using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VSS.MasterData.WebAPI.ClientModel;

namespace DeviceSettings.Controller
{
	[Route("v1/EventStores")]
	public class EventStoresController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		public EventStoresController(IConfiguration configuration)
		{
			_configuration = configuration;

		}

		[Route("")]
		[HttpGet]
		public Definitions GetTopicMetaData()
		{
			var definition = new Definitions { name = "VSPKafkaTopic" };

			var deviceTopic = GetDeviceTopic();

			definition.Topics = new[] { deviceTopic };

			return definition;
		}

		private DefinitionsTopic GetDeviceTopic()
		{
			var deviceTopic = new DefinitionsTopic
			{
				DefaultKey = "DeviceUID",
				Name = _configuration["KafkaTopicName"],
				URL = _configuration["RestProxyBaseUrl"]
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
	}
}
