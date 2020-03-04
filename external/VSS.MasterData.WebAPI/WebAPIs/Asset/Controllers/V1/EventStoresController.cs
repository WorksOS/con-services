using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;
using VSS.MasterData.WebAPI.ClientModel;

namespace VSS.MasterData.WebAPI.Asset.Controllers.V1
{
	[ExcludeFromCodeCoverage]
	[Route("v1/EventStores")]
	public class EventStoresController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		public EventStoresController(IConfiguration configuration)
		{
			_configuration = configuration;

		}
		#region Public Methods

		/// <summary>
		/// 
		/// </summary>
		/// <returns>Definitions</returns>
		[Route("")]
		[HttpGet]
		public Definitions GetTopicMetaData()
		{
			var definition = new Definitions { name = "VSSKafkaTopic" };

			var assetTopic = GetAssetTopic();

			var workdefinitionTopic = GetWorkDefinitionTopic();

			var assetOwnerTopic = GetAssetOwnerTopic();

			var assetEcmInfoTopic = GetAssetEcmInfoTopic();

			definition.Topics = new[] { assetTopic, workdefinitionTopic, assetOwnerTopic, assetEcmInfoTopic };

			return definition;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>DefinitionsTopic</returns>
		private DefinitionsTopic GetAssetTopic()
		{

			var assetTopic = new DefinitionsTopic
			{
				DefaultKey = "AssetUID",
				Name = _configuration["AssetTopicNames"],
				URL = _configuration["RestProxyBaseUrl"]
			};

			var assetCreateEvent = new DefinitionsTopicEvent
			{
				Name = "CreateAssetEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
				  "{\"CreateAssetEvent\": \"{\"AssetName\": \"string\",  \"LegacyAssetID\": \"number\",\"SerialNumber\": \"string\",\"MakeCode\": \"string\",\"Model\": \"string\",\"AssetType\": \"string\",\"IconKey\": \"number\",\"EquipmentVIN\":\"string\",\"ModelYear\":\"number\",\"AssetUID\":\"string\",\"OwningCustomerUID\": \"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var assetUpdateEvent = new DefinitionsTopicEvent
			{
				Name = "UpdateAssetEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
				  "{\"UpdateAssetEvent\": \"{\"AssetName\": \"string\", \"LegacyAssetID\": \"number\",\"Model\": \"string\",\"AssetType\": \"string\",\"IconKey\": \"number\",\"EquipmentVIN\":\"string\",\"ModelYear\":\"number\",\"OwningCustomerUID\": \"string\",\"AssetUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var assetDeleteEvent = new DefinitionsTopicEvent
			{
				Name = "DeleteAssetEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
				  "{\"DeleteAssetEvent\": \"{\"AssetUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};
			assetTopic.Event = new[] { assetCreateEvent, assetUpdateEvent, assetDeleteEvent };
			return assetTopic;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>DefinitionsTopic</returns>
		private DefinitionsTopic GetWorkDefinitionTopic()
		{
			var workdefinitionTopic = new DefinitionsTopic
			{
				DefaultKey = "AssetUID",
				Name = _configuration["WorkDefinitionTopicNames"],
				URL = _configuration["RestProxyBaseUrl"]
			};

			var workdefinitionCreateEvent = new DefinitionsTopicEvent
			{
				Name = "CreateWorkDefinitionEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
				  "{\"CreateWorkDefinitionEvent\": \"{\"AssetUID\": \"string\",\"WorkDefinitionType\": \"string\",\"SensorNumber\": \"string\",\"StartIsOn\": \"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var workdefinitionUpdateEvent = new DefinitionsTopicEvent
			{
				Name = "UpdateWorkDefinitionEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
				  "{\"UpdateWorkDefinitionEvent\": \"{\"AssetUID\": \"string\",\"WorkDefinitionType\": \"string\",\"SensorNumber\": \"string\",\"StartIsOn\": \"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};
			workdefinitionTopic.Event = new DefinitionsTopicEvent[] { workdefinitionCreateEvent, workdefinitionUpdateEvent };
			return workdefinitionTopic;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>DefinitionsTopic</returns>
		private DefinitionsTopic GetAssetOwnerTopic()
		{
			var assetOwnerTopic = new DefinitionsTopic
			{
				DefaultKey = "AssetUID",
				Name = _configuration["AssetOwnerTopicName"],
				URL = _configuration["RestProxyBaseUrl"]
			};

			var assetOwnerEvent = new DefinitionsTopicEvent
			{
				Name = "AssetOwnerEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
				  "{\"AssetUID\": \"string\",\"AssetOwnerRecord\": {\"CustomerName\":\"string\",\"DealerAccountCode\":\"string\",\"DealerUID\":\"string\",\"DealerName\":\"string\",\"NetworkDealerCode\":\"string\",\"NetworkCustomerCode\":\"string\",\"CustomerUID\":\"string\"},\"Action\": \"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}"
			};

			assetOwnerTopic.Event = new DefinitionsTopicEvent[] { assetOwnerEvent };
			return assetOwnerTopic;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>DefinitionsTopic</returns>
		private DefinitionsTopic GetAssetEcmInfoTopic()
		{
			var assetOwnerTopic = new DefinitionsTopic
			{
				DefaultKey = "AssetUID",
				Name = _configuration["AssetEcmInfoTopicName"],
				URL = _configuration["RestProxyBaseUrl"]
			};

			var assetOwnerEvent = new DefinitionsTopicEvent
			{
				Name = "AssetECMInfoEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
				  "{\"AssetUID\": \"string\",\"AssetECMInfo\": {\"ECMSerialNumber\":\"string\",\"FirmwarePartNumber\":\"string\",\"ECMDescription\":\"string\",\"SynClockEnabled\":\"int\",\"SyncClockLevel\":\"string\"},\"Action\": \"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}"
			};

			assetOwnerTopic.Event = new DefinitionsTopicEvent[] { assetOwnerEvent };
			return assetOwnerTopic;
		}

		#endregion Public Methods
	}
}
