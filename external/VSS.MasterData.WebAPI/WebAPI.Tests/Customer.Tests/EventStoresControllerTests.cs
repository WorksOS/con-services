using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.IO;
using System.Linq;
using System.Reflection;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.Customer.Controllers.V1;
using Xunit;

namespace VSS.MasterData.WebAPI.Customer.Tests
{
	public class EventStoresControllerTests
	{
		private readonly IConfiguration configuration;
		private static string Name;
		private static string URL;
		public EventStoresControllerTests()
		{
			string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			configuration = new ConfigurationBuilder().SetBasePath(currentDirectory)
													.AddJsonFile("appsettings.json", true)
													.AddEnvironmentVariables()
													.Build();
			Name = configuration["CustomerTopicNames"];
			URL = configuration["RestProxyBaseUrl"];
		}
		[Fact]
		public void TestGetTopicMetaData_ExpectSuccess()
		{
			var expectedDefinition = new Definitions { name = "VSPKafkaTopic" };
			var expectedCustomerTopicMetaData = GetCustomerTopic();
			var controller = new EventStoresController(configuration);
			var actualCustomerTopicMetaData = controller.GetTopicMetaData();

			Assert.Equal(expectedDefinition.name, actualCustomerTopicMetaData.name);
			Assert.Equal(expectedCustomerTopicMetaData.DefaultKey, actualCustomerTopicMetaData.Topics[0].DefaultKey);
			var eventsCount = actualCustomerTopicMetaData.Topics[0].Event.Count();
			for (int j = 0; j < eventsCount; j++)
			{
				Assert.Equal(expectedCustomerTopicMetaData.Event[j].Encoding, actualCustomerTopicMetaData.Topics[0].Event[j].Encoding);
				Assert.Equal(expectedCustomerTopicMetaData.Event[j].MessageFormat, actualCustomerTopicMetaData.Topics[0].Event[j].MessageFormat);
				Assert.Equal(expectedCustomerTopicMetaData.Event[j].Name, actualCustomerTopicMetaData.Topics[0].Event[j].Name);
				Assert.Equal(expectedCustomerTopicMetaData.Event[j].PayloadFormat, actualCustomerTopicMetaData.Topics[0].Event[j].PayloadFormat);
			}
			Assert.Equal(expectedCustomerTopicMetaData.Name, actualCustomerTopicMetaData.Topics[0].Name);
			Assert.Equal(expectedCustomerTopicMetaData.URL, actualCustomerTopicMetaData.Topics[0].URL);
		}

		[Fact]
		public void TestGetTopicMetaData_WrongTopicName_ExpectFailure()
		{
			var expectedCustomerTopicMetaData = GetCustomerTopic();
			var controller = new EventStoresController(configuration);
			var actualCustomerTopicMetaData = controller.GetTopicMetaData();

			Assert.Equal(expectedCustomerTopicMetaData.DefaultKey, actualCustomerTopicMetaData.Topics[0].DefaultKey);
			var eventsCount = actualCustomerTopicMetaData.Topics[0].Event.Count();
			actualCustomerTopicMetaData.Topics[0].Name = "InvalidTopicName";
			for (int j = 0; j < eventsCount; j++)
			{
				Assert.Equal(expectedCustomerTopicMetaData.Event[j].Encoding, actualCustomerTopicMetaData.Topics[0].Event[j].Encoding);
				Assert.Equal(expectedCustomerTopicMetaData.Event[j].MessageFormat, actualCustomerTopicMetaData.Topics[0].Event[j].MessageFormat);
				Assert.Equal(expectedCustomerTopicMetaData.Event[j].Name, actualCustomerTopicMetaData.Topics[0].Event[j].Name);
				Assert.Equal(expectedCustomerTopicMetaData.Event[j].PayloadFormat, actualCustomerTopicMetaData.Topics[0].Event[j].PayloadFormat);
			}
			Assert.Equal(expectedCustomerTopicMetaData.URL, actualCustomerTopicMetaData.Topics[0].URL);
			Assert.NotEqual(expectedCustomerTopicMetaData.Name, actualCustomerTopicMetaData.Topics[0].Name);
		}

		private static DefinitionsTopic GetCustomerTopic()
		{
			var customerTopic = new DefinitionsTopic
			{
				DefaultKey = "CustomerUID",
				Name = Name,
				URL = URL
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
				PayloadFormat = "{\"CreateCustomerRelationshipEvent\": \"{\"ParentCustomerUID\":\"string\",\"ChildCustomerUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var deleteCustomerRelationshipEvent = new DefinitionsTopicEvent
			{
				Name = "DeleteCustomerRelationShipEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat = "{\"DeleteCustomerRelationShipEvent\": \"{\"ParentCustomerUID\":\"string\",\"ChildCustomerUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\",\"ReceivedUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			customerTopic.Event = new[] { customerCreateEvent, customerUpdateEvent, customerDeleteEvent,
		associateCustomerAssetEvent, dissociateCustomerAssetEvent, associateCustomerUserEvent, dissociateCustomerUserEvent,createCustomerRelationshipEvent,deleteCustomerRelationshipEvent };

			return customerTopic;
		}
	}
}
