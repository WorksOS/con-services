using System.Linq;
using Xunit;
using System.Configuration;
using VSS.MasterData.WebAPI.Device.Controllers.V1;
using VSS.MasterData.WebAPI.ClientModel;

namespace VSP.MasterData.Device.UnitTests
{
	public class EventStoresControllerTests
	{
		private readonly EventStoresController _target;

        public EventStoresControllerTests()
		{
            _target = new EventStoresController();
		}

        [Fact]
        public void TestEventStoresController_Success()
        {
            var result = _target.GetTopicMetaData();
            Assert.NotNull(result);
            Assert.IsType<Definitions>(result);
            // 6 Device Events So far
            Assert.Equal(6,result.Topics.First().Event.Count());
            Assert.Equal(ConfigurationManager.AppSettings["KafkaTopicName"], result.Topics.First().Name);
        }

		[Fact]
		public void TestEventStoresController_Success_All()
		{
			var expectedDefinition = new Definitions { name = "VSSKafkaTopic" };
			var expectedCustomerTopicMetaData = GetDeviceTopic();
			var controller = new EventStoresController();
			var actualCustomerTopicMetaData = controller.GetTopicMetaData();

			Assert.Equal(expectedDefinition.name, actualCustomerTopicMetaData.name);
			Assert.Equal(expectedCustomerTopicMetaData.DefaultKey, actualCustomerTopicMetaData.Topics[0].DefaultKey);
			var eventsCount = actualCustomerTopicMetaData.Topics[0].Event.Count();
			for (int index = 0; index < eventsCount; index++)
			{
				Assert.Equal(expectedCustomerTopicMetaData.Event[index].Encoding, actualCustomerTopicMetaData.Topics[0].Event[index].Encoding);
				Assert.Equal(expectedCustomerTopicMetaData.Event[index].MessageFormat, actualCustomerTopicMetaData.Topics[0].Event[index].MessageFormat);
				Assert.Equal(expectedCustomerTopicMetaData.Event[index].Name, actualCustomerTopicMetaData.Topics[0].Event[index].Name);
				Assert.Equal(expectedCustomerTopicMetaData.Event[index].PayloadFormat, actualCustomerTopicMetaData.Topics[0].Event[index].PayloadFormat);
			}
			Assert.Equal(expectedCustomerTopicMetaData.Name, actualCustomerTopicMetaData.Topics[0].Name);
			Assert.Equal(expectedCustomerTopicMetaData.URL, actualCustomerTopicMetaData.Topics[0].URL);
		}

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
	}
}
