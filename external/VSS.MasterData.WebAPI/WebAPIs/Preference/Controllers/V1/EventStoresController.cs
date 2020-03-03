using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace VSS.MasterData.WebAPI.Preference.Controllers.V1
{
	/// <summary>
	/// Controller to return Kafka Message structure of Preference Events
	/// </summary>
	[Route("v1/EventStores")]
	[ExcludeFromCodeCoverage]
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
			var definition = new Definitions { Name = "VSSKafkaTopic" };

			var preferenceTopic = GetPreferenceTopic();

			definition.Topics = new[] { preferenceTopic };

			return definition;
		}

		private DefinitionsTopic GetPreferenceTopic()
		{
			var preferenceTopic = new DefinitionsTopic
			{
				DefaultKey = "PreferenceKeyUID for PreferenceKey events And UserUID for UserPreference events",
				Name = configuration["PreferenceKafkaTopicNames"],
				URL = configuration["RestProxyBaseUrl"]
			};


			var createUserPreferenceEvent = new DefinitionsTopicEvent
			{
				Name = "CreateUserPreferenceEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"CreateUserPreferenceEvent\": \"{\"UserUID\": \"string\",\"PreferenceKeyUID\": \"string\",\"PreferenceKeyName\": \"string\",\"PreferenceJson\": \"string\",\"SchemaVersion\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var updateUserPreferenceEvent = new DefinitionsTopicEvent
			{
				Name = "UpdateUserPreferenceEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"UpdateUserPreferenceEvent\": \"{\"UserUID\": \"string\",\"PreferenceKeyUID\": \"string\",\"PreferenceKeyName\": \"string\",\"PreferenceJson\": \"string\",\"SchemaVersion\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var deleteUserPreferenceEvent = new DefinitionsTopicEvent
			{
				Name = "DeleteUserPreferencePayload",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"DeleteUserPreferencePayload\": \"{\"PreferenceKeyUID\":\"string\",\"PreferenceKeyName\":\"string\"}\"}"
			};

			var createPreferenceKeyEvent = new DefinitionsTopicEvent
			{
				Name = "CreatePreferenceKeyEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"CreatePreferenceKeyEvent\": \"{\"PreferenceKeyName\":\"string\",\"PreferenceKeyUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var updatePreferenceKeyEvent = new DefinitionsTopicEvent
			{
				Name = "UpdatePreferenceKeyEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"UpdatePreferenceKeyEvent\": \"{\"PreferenceKeyUID\":\"string\",\"PreferenceKeyName\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			var deletePreferenceKeyEvent = new DefinitionsTopicEvent
			{
				Name = "DeletePreferenceKeyEvent",
				MessageFormat = "JSON",
				Encoding = "UTF-8",
				PayloadFormat =
					"{\"DeletePreferenceKeyEvent\": \"{\"PreferenceKeyUID\":\"string\",\"ActionUTC\":\"yyyy-MM-ddTHH:mm:ss.ffffK\"}\"}"
			};

			preferenceTopic.Event = new[]
			{
				createUserPreferenceEvent, updateUserPreferenceEvent, deleteUserPreferenceEvent,
				createPreferenceKeyEvent, updatePreferenceKeyEvent, deletePreferenceKeyEvent
			};
			return preferenceTopic;
		}
	}
}