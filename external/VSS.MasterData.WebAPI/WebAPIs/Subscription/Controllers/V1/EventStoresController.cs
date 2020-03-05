using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VSS.MasterData.WebAPI.Subscription.Controllers.V1
{
	/// <summary>
	/// Controller to return Kafka Message structure of Subscription Events
	/// </summary>
	[Route("v1/EventStores")]
	public class EventStoresController : ControllerBase
	{
		private readonly IConfiguration configuration;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="configuration"></param>
		public EventStoresController(IConfiguration configuration)
		{
			this.configuration = configuration;
		}

		/// <summary>
		/// GetTopicMetaData
		/// </summary>
		/// <returns></returns>
		[Route("")]
		[HttpGet]
		public Definitions GetTopicMetaData()
		{
			var definition = new Definitions {name = "VSSKafkaTopic"};

			var subscriptionTopic = GetSubscriptionTopic();

			definition.Topics = new[] {subscriptionTopic};

			return definition;
		}

		private DefinitionsTopic GetSubscriptionTopic()
		{
			var subscriptionTopic = new DefinitionsTopic
			{
				DefaultKey = "SubscriptionUID",
				Name = configuration["SubscriptionKafkaTopicNames"],
				URL = configuration["RestProxyBaseUrl"]
			};


			var assetSubscriptionCreateEvent = new DefinitionsTopicEvent
			{
				Name = "CreateAssetSubscriptionEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"CreateAssetSubscriptionEvent\": \"{\"SubscriptionUID\": \"string\",\"CustomerUID\": \"string\",\"AssetUID\": \"string\",\"DeviceUID\": \"string\",\"SubscriptionType\": \"string\",\"Source\": \"string\",\"StartDate\": \"yyyy-MM-ddTHH:mm:ss.ffffK\",\"EndDate\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var assetSubscriptionUpdateEvent = new DefinitionsTopicEvent
			{
				Name = "UpdateAssetSubscriptionEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"UpdateAssetSubscriptionEvent\": \"{\"SubscriptionUID\": \"string\",\"CustomerUID\": \"string\",\"AssetUID\": \"string\",\"DeviceUID\": \"string\",\"SubscriptionType\": \"string\",\"Source\": \"string\",\"StartDate\": \"yyyy-MM-ddTHH:mm:ss.ffffK\",\"EndDate\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var projectSubscriptionCreateEvent = new DefinitionsTopicEvent
			{
				Name = "CreateProjectSubscriptionEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"CreateProjectSubscriptionEvent\": \"{\"SubscriptionUID\": \"string\",\"CustomerUID\": \"string\",\"SubscriptionType\": \"string\",\"StartDate\": \"yyyy-MM-ddTHH:mm:ss.ffffK\",\"EndDate\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var projectSubscriptionUpdateEvent = new DefinitionsTopicEvent
			{
				Name = "UpdateProjectSubscriptionEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"UpdateProjectSubscriptionEvent\": \"{\"SubscriptionUID\": \"string\",\"CustomerUID\": \"string\",\"SubscriptionType\": \"string\",\"StartDate\": \"yyyy-MM-ddTHH:mm:ss.ffffK\",\"EndDate\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var associateProjectSubscriptionEvent = new DefinitionsTopicEvent
			{
				Name = "AssociateProjectSubscriptionEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"AssociateProjectSubscriptionEvent\": \"{\"SubscriptionUID\": \"string\",\"ProjectUID\": \"string\",\"EffectiveDate\": \"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var dissociateProjectSubscriptionEvent = new DefinitionsTopicEvent
			{
				Name = "DissociateProjectSubscriptionEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"DissociateProjectSubscriptionEvent\": \"{\"SubscriptionUID\": \"string\",\"ProjectUID\": \"string\",\"EffectiveDate\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var customerSubscriptionCreateEvent = new DefinitionsTopicEvent
			{
				Name = "CreateCustomerSubscriptionEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"CreateCustomerSubscriptionEvent\": \"{\"SubscriptionUID\": \"string\",\"CustomerUID\": \"string\",\"SubscriptionType\": \"string\",\"StartDate\": \"yyyy-MM-ddTHH:mm:ss.ffffK\",\"EndDate\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var customerSubscriptionUpdateEvent = new DefinitionsTopicEvent
			{
				Name = "UpdateCustomerSubscriptionEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"UpdateCustomerSubscriptionEvent\": \"{\"SubscriptionUID\": \"string\",\"CustomerUID\": \"string\",\"SubscriptionType\": \"string\",\"StartDate\": \"yyyy-MM-ddTHH:mm:ss.ffffK\",\"EndDate\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			subscriptionTopic.Event = new DefinitionsTopicEvent[]
			{
				assetSubscriptionCreateEvent, assetSubscriptionUpdateEvent, projectSubscriptionCreateEvent,
				projectSubscriptionUpdateEvent,
				associateProjectSubscriptionEvent, dissociateProjectSubscriptionEvent, customerSubscriptionCreateEvent,
				customerSubscriptionUpdateEvent
			};
			return subscriptionTopic;
		}
	}
}