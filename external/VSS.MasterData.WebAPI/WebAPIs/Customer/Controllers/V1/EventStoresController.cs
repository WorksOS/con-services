using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VSS.MasterData.WebAPI.ClientModel;

namespace VSS.MasterData.WebAPI.Customer.Controllers.V1
{
	/// <summary>
	/// Customer API Kafka events metadata details
	/// </summary>
	[Route("v1/EventStores")]
	public class EventStoresController : ControllerBase
	{
		private readonly IConfiguration configuration;

		/// <summary>
		/// Events constructor
		/// </summary>
		/// <param name="configuration"></param>
		public EventStoresController(IConfiguration configuration)
		{
			this.configuration = configuration;
		}

		/// <summary>
		/// Get Customer/Account kafka topics metadata
		/// </summary>
		/// <returns></returns>
		[Route("")]
		[HttpGet]
		public Definitions GetTopicMetaData()
		{
			var definition = new Definitions {name = "VSPKafkaTopic"};

			var customerTopic = GetCustomerTopic();
			var accountTopic = GetAccountTopic();

			definition.Topics = new[] {customerTopic, accountTopic};

			return definition;
		}

		private DefinitionsTopic GetCustomerTopic()
		{
			var customerTopic = new DefinitionsTopic
			{
				DefaultKey = "CustomerUID",
				Name = configuration["CustomerTopicNames"],
				URL = configuration["RestProxyBaseUrl"]
			};

			var customerCreateEvent = new DefinitionsTopicEvent
			{
				Name = "CreateCustomerEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"CreateCustomerEvent\":\"{\"CustomerName\":\"string\",\"CustomerType\":\"string\",\"BSSID\":\"string\",\"DealerNetwork\":\"string\",\"NetworkDealerCode\":\"string\",\"NetworkCustomerCode\":\"string\",\"DealerAccountCode\":\"string\",\"CustomerUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var customerUpdateEvent = new DefinitionsTopicEvent
			{
				Name = "UpdateCustomerEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"UpdateCustomerEvent\":\"{\"CustomerName\":\"string\",\"DealerNetwork\":\"string\",\"NetworkDealerCode\":\"string\",\"NetworkCustomerCode\":\"string\",\"DealerAccountCode\":\"string\",\"CustomerUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var customerDeleteEvent = new DefinitionsTopicEvent
			{
				Name = "DeleteCustomerEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"DeleteCustomerEvent\":\"{\"CustomerUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var associateCustomerAssetEvent = new DefinitionsTopicEvent
			{
				Name = "AssociateCustomerAssetEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"AssociateCustomerAssetEvent\":\"{\"CustomerUID\":\"string\",\"RelationType\":\"string\",\"AssetUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var associateCustomerUserEvent = new DefinitionsTopicEvent
			{
				Name = "AssociateCustomerUserEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"AssociateCustomerUserEvent\":\"{\"CustomerUID\":\"string\",\"UserUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var dissociateCustomerAssetEvent = new DefinitionsTopicEvent
			{
				Name = "DissociateCustomerAssetEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"DissociateCustomerAssetEvent\":\"{\"CustomerUID\":\"string\",\"AssetUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var dissociateCustomerUserEvent = new DefinitionsTopicEvent
			{
				Name = "DissociateCustomerUserEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"DissociateCustomerUserEvent\": \"{\"CustomerUID\":\"string\",\"UserUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var createCustomerRelationshipEvent = new DefinitionsTopicEvent
			{
				Name = "CreateCustomerRelationshipEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"CreateCustomerRelationshipEvent\": \"{\"ParentCustomerUID\":\"string\",\"ChildCustomerUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var deleteCustomerRelationshipEvent = new DefinitionsTopicEvent
			{
				Name = "DeleteCustomerRelationShipEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"DeleteCustomerRelationShipEvent\": \"{\"ParentCustomerUID\":\"string\",\"ChildCustomerUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			customerTopic.Event = new[]
			{
				customerCreateEvent,
				customerUpdateEvent,
				customerDeleteEvent,
				associateCustomerAssetEvent,
				dissociateCustomerAssetEvent,
				associateCustomerUserEvent,
				dissociateCustomerUserEvent,
				createCustomerRelationshipEvent,
				deleteCustomerRelationshipEvent
			};
			return customerTopic;
		}

		private DefinitionsTopic GetAccountTopic()
		{
			var accountTopic = new DefinitionsTopic
			{
				DefaultKey = "AccountUID",
				Name = configuration["AccountTopicName"],
				URL = configuration["RestProxyBaseUrl"]
			};

			var createAccountEvent = new DefinitionsTopicEvent
			{
				Name = "AccountEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"AccountEvent\":\"{\"AccountName\":\"string\",\"BSSID\":\"string\",\"NetworkCustomerCode\":\"string\",\"DealerAccountCode\":\"string\",\"Action\":\"string\",\"fk_ParentCustomerUID\":\"string\",\"fk_ChildCustomerUID\":\"string\",\"AccountUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var updateAccountEvent = new DefinitionsTopicEvent
			{
				Name = "AccountEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"AccountEvent\":\"{\"AccountName\":\"string\",\"BSSID\":\"string\",\"NetworkCustomerCode\":\"string\",\"DealerAccountCode\":\"string\",\"Action\":\"Update\",\"fk_ParentCustomerUID\":\"string\",\"fk_ChildCustomerUID\":\"string\",\"AccountUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var deleteAccountEvent = new DefinitionsTopicEvent
			{
				Name = "AccountEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"AccountEvent\":\"{\"AccountName\":\"string\",\"BSSID\":\"string\",\"NetworkCustomerCode\":\"string\",\"DealerAccountCode\":\"string\",\"Action\":\"Delete\",\"fk_ParentCustomerUID\":\"string\",\"fk_ChildCustomerUID\":\"string\",\"AccountUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			accountTopic.Event = new[]
			{
				createAccountEvent,
			};
			return accountTopic;
		}
	}
}