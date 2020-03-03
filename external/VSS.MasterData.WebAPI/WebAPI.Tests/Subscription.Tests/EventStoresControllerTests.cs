using System.Linq;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using VSS.MasterData.WebAPI.Subscription.Controllers.V1;
using Xunit;

namespace VSS.MasterData.WebAPI.Subscription.Tests
{
	public class EventStoresControllerTests
	{
		private readonly EventStoresController controller;
		private readonly static IConfiguration configuration = Substitute.For<IConfiguration>();

		public EventStoresControllerTests()
		{
			controller = new EventStoresController(configuration);
		}

		[Fact]
		public void TestEventStoresController_Success()
		{
			var expectedDefinition = new Definitions { name = "VSSKafkaTopic" };
			var expectedCustomerTopicMetaData = GetSubscriptionTopic();
			var controller = new EventStoresController(configuration);
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

		private static DefinitionsTopic GetSubscriptionTopic()
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

			subscriptionTopic.Event = new DefinitionsTopicEvent[] { assetSubscriptionCreateEvent, assetSubscriptionUpdateEvent, projectSubscriptionCreateEvent, projectSubscriptionUpdateEvent,
			associateProjectSubscriptionEvent, dissociateProjectSubscriptionEvent, customerSubscriptionCreateEvent, customerSubscriptionUpdateEvent};
			return subscriptionTopic;
		}
	}
}