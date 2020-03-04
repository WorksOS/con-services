using ClientModel.WorkDefinition;
using CommonModel.AssetSettings;
using DbModel.WorkDefinition;
using Interfaces;
using KafkaModel;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace WorkDefinitionRepository
{
	public class WorkDefinitionServices : IWorkDefinitionServices
	{
		private readonly ILoggingService _loggingService;
		private readonly ITransactions _transaction;
		private readonly IOptions<Configurations> _configuration;
		private readonly List<string> workdefintionTopicNames;

		public WorkDefinitionServices(ITransactions transactions, IOptions<Configurations> configurations, ILoggingService loggingService)
		{
			_transaction = transactions;
			_configuration = configurations;
			workdefintionTopicNames = _configuration.Value.KafkaSettings.PublisherTopics.WorkDefinitionTopicNames.Split(',').ToList();
			_loggingService = loggingService;
		}

		public bool CreateWorkDefinition(WorkDefinitionEvent workDefinitionEvent)
		{
			var workDefinition = GetDbModel(workDefinitionEvent);

			_loggingService.Info($"Create Work Definition: {JsonConvert.SerializeObject(workDefinition)}", "WorkDefinitionServices.CreateWorkDefinition");

			var messages = new List<KafkaMessage>(workdefintionTopicNames?.Select((topic) =>		
				new KafkaMessage
				{
					Key = workDefinition.AssetUID.ToString(),
					Message = new
					{
						CreateWorkDefinitionEvent = new
						{
							AssetUID = workDefinition.AssetUID.ToString(),
							WorkDefinitionType = workDefinitionEvent.WorkDefinitionType,
							SensorNumber = workDefinition.SwitchNumber,
							StartIsOn = workDefinition.SwitchWorkStartState,
							ActionUTC = workDefinition.InsertUTC,
							ReceivedUTC = workDefinition.InsertUTC,
						}
					},
					Topic = topic
				}
			));
			
			var actions = new List<Action>()
					{
						() => _transaction.Upsert(workDefinition),
						() => _transaction.Publish(messages)
					};

			return _transaction.Execute(actions);
		}

		public bool UpdateWorkDefinition(WorkDefinitionEvent workDefinitionEvent)
		{
			var workDefinition = GetDbModel(workDefinitionEvent);
			_loggingService.Info($"Update Work Definition: {JsonConvert.SerializeObject(workDefinition)}", "WorkDefinitionServices.UpdateWorkDefinition");

			var messages = new List<KafkaMessage>(workdefintionTopicNames?.Select((topic) => 
				new KafkaMessage
				{
					Key = workDefinition.AssetUID.ToString(),
					Message = new
					{
						UpdateWorkDefinitionEvent = new
						{
							AssetUID = workDefinition.AssetUID.ToString(),
							WorkDefinitionType = workDefinitionEvent.WorkDefinitionType,
							SensorNumber = workDefinition.SwitchNumber,
							StartIsOn = workDefinition.SwitchWorkStartState,
							ActionUTC = workDefinition.InsertUTC,
							ReceivedUTC = workDefinition.InsertUTC,
						}
					},
					Topic = topic
				}));

			var actions = new List<Action>()
					{
						() => _transaction.Upsert(workDefinition),
						() => _transaction.Publish(messages)
					};

			return _transaction.Execute(actions);
		}

		public WorkDefinitionDto GetWorkDefinition(Guid assetUID)
		{
			string query = @"SELECT awd.fk_AssetUID as AssetUID, awd.InsertUTC, awd.SwitchNumber, awd.SwitchWorkStartState, wdt.Description WorkDefinitionType 
							FROM md_asset_AssetWorkDefinition awd 
							INNER JOIN md_asset_WorkDefinitionType wdt ON wdt.WorkDefinitionTypeID = awd.fk_WorkDefinitionTypeID WHERE awd.fk_AssetUID = UNHEX('{0}') 
							ORDER BY awd.StartDate DESC LIMIT 1;";
			var result = _transaction.Get<WorkDefinitionDto>(string.Format(query, assetUID.ToString("N")));
			return result.FirstOrDefault();
		}

		public bool WorkDefinitionExist(Guid assetUID)
		{
			string query = @"SELECT COUNT(1) FROM md_asset_AssetWorkDefinition WHERE fk_AssetUID = UNHEX('{0}');";
			var ExistingWorkDefinition = _transaction.Get<string>(string.Format(query, assetUID.ToString("N"))).FirstOrDefault();
			return ExistingWorkDefinition != null && ExistingWorkDefinition.Count() > 0;
			throw new Exception($"WorkDefinition: {ExistingWorkDefinition} does not exist");
		}

		public long GetWorkDefinitionTypeID(string workDefinitionType)
		{
			string query = @"SELECT WorkDefinitionTypeID from md_asset_WorkDefinitionType WHERE Description = '{0}';";
			var workDefinitionID = _transaction.Get<string>(string.Format(query, workDefinitionType)).FirstOrDefault();
			if (workDefinitionID != null && workDefinitionID.Count() > 0)
				return long.Parse(workDefinitionID);
			return 0;
		}

		private WorkDefinitionDto GetDbModel(WorkDefinitionEvent workDefinitionEvent)
		{
			DateTime utcNow = DateTime.UtcNow;
			return new WorkDefinitionDto()
			{
				AssetUID = workDefinitionEvent.AssetUID,
				WorkDefinitionTypeID = GetWorkDefinitionTypeID(workDefinitionEvent.WorkDefinitionType),
				SwitchNumber = workDefinitionEvent.SensorNumber,
				SwitchWorkStartState = workDefinitionEvent.StartIsOn,
				StartDate = workDefinitionEvent.ReceivedUTC,
				InsertUTC = workDefinitionEvent.ReceivedUTC,
				UpdateUTC = workDefinitionEvent.ReceivedUTC
			};
		}
	}
}
