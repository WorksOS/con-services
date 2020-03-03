using ClientModel.AssetSettings.Request;
using CommonModel.AssetSettings;
using DbModel.AssetSettings;
using CommonModel.Enum;
using Infrastructure.Service.AssetSettings.Interfaces;
using KafkaModel;
using KafkaModel.AssetSettings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace Infrastructure.Service.AssetSettings.Publisher
{
	public class AssetSettingsPublisher : IAssetSettingsPublisher
	{
		private readonly string _userAssetSettingsTopic;
		private readonly string _userAssetWeeklySettingsTopic;
		private readonly string _assetSettingsTopic;
		private readonly string _assetWeeklySettingsTopic;
		private readonly ITransactions _transactions;
		private readonly ILoggingService _loggingService;

		public AssetSettingsPublisher(IOptions<Configurations> configurations, ITransactions transactions, ILoggingService loggingService)
		{
			this._assetWeeklySettingsTopic = configurations.Value.KafkaSettings.PublisherTopics.AssetWeeklySettingsTopicName;
			this._userAssetWeeklySettingsTopic = configurations.Value.KafkaSettings.PublisherTopics.UserAssetWeeklySettingsTopicName;
			this._assetSettingsTopic = configurations.Value.KafkaSettings.PublisherTopics.AssetSettingsTopicName;
			this._userAssetSettingsTopic = configurations.Value.KafkaSettings.PublisherTopics.UserAssetSettingsTopicName;
			this._transactions = transactions;
			this._loggingService = loggingService;
			this._loggingService.CreateLogger<AssetSettingsPublisher>();
		}

		public bool publishAssetWeeklySettings(List<AssetWeeklyTargetsDto> targets)
		{
			try
			{
				_loggingService.Debug("Started Publishing Asset Weekly Settings", "AssetSettingsPublisher.publishAssetWeeklySettings");
				List<KafkaMessage> msgs = new List<KafkaMessage>();
				foreach (var target in targets)
				{
					_loggingService.Debug(string.Format("Current Target : {0}", JsonConvert.SerializeObject(target)), "AssetSettingsPublisher.publishAssetWeeklySettings");
					var createAssetWeeklyTargetEvent = new AssetWeeklyTargetEvent
					{
						AssetTargetUID = target.AssetTargetUID,
						AssetUID = target.AssetUID,
						EndDate = target.EndDate,
						StartDate = target.StartDate,
						SundayTargetValue = target.SundayTargetValue,
						MondayTargetValue = target.MondayTargetValue,
						TuesdayTargetValue = target.TuesdayTargetValue,
						WednesdayTargetValue = target.WednesdayTargetValue,
						ThursdayTargetValue = target.ThursdayTargetValue,
						FridayTargetValue = target.FridayTargetValue,
						SaturdayTargetValue = target.SaturdayTargetValue,
						TargetType = target.TargetType,
						InsertUTC = DateTime.UtcNow,
						StatusInd = target.Status,
						UpdateUTC = DateTime.UtcNow
					};
					_loggingService.Debug(string.Format("Message To Be Published to Kafka :- {0} ", JsonConvert.SerializeObject(new KafkaMessage { Key = target.AssetUID.ToString(), Message = createAssetWeeklyTargetEvent, Topic = _assetWeeklySettingsTopic })), "AssetSettingsPublisher.publishAssetWeeklySettings");
					msgs.Add(new KafkaMessage { Key = target.AssetUID.ToString(), Message = createAssetWeeklyTargetEvent, Topic = _assetWeeklySettingsTopic });
				}
				_transactions.Publish(msgs);
				_loggingService.Debug(string.Format("Weekly Settings Published to Kafka :- {0} ", JsonConvert.SerializeObject(msgs)), "AssetSettingsPublisher.publishAssetWeeklySettings");
				return true;
			}
			catch (Exception ex)
			{
				this._loggingService.Error("An Exception has occurred ", MethodInfo.GetCurrentMethod().Name, ex);
				return false;
			}
		}

		public void PublishUserAssetSettings(AssetSettingsRequestBase request)
		{
			try
			{
				var payloadMessages = new List<KafkaMessage>();
				_loggingService.Debug("Started Publishing User Asset Settings", "AssetSettingsPublisher.PublishUserAssetSettings");
				foreach (var assetUId in request.AssetUIds)
				{
					foreach (var targetValue in request.TargetValues)
					{
						var userAssetTargetEvent = new UserAssetTargetEvent
						{
							AssetUID = Guid.Parse(assetUId),
							TargetType = targetValue.Key,
							StartDate = Convert.ToDateTime(request.StartDate),
							Timestamp = new TimestampDetail { EventUtc = DateTime.UtcNow },
							UserUID = request.UserUid,
							CustomerUID = request.CustomerUid,
							TargetValue = targetValue.Value
						};
						var payloadMessage = new KafkaMessage { Key = userAssetTargetEvent.AssetUID.ToString(), Message = userAssetTargetEvent, Topic = _userAssetSettingsTopic };
						payloadMessages.Add(payloadMessage); // check we need to publish with hypens or not
						_loggingService.Debug(string.Format("USer Asset Settings Published to Kafka :- {0} ", JsonConvert.SerializeObject(payloadMessage)), "AssetSettingsPublisher.PublishUserAssetSettings");
					}
				}
				_transactions.Publish(payloadMessages);
				_loggingService.Debug("Ended Publishing User Asset Settings", "AssetSettingsPublisher.PublishUserAssetSettings");
			}
			catch (Exception ex)
			{
				this._loggingService.Error("An Exception has occurred ", MethodInfo.GetCurrentMethod().Name, ex);
			}
		}

		public void PublishUserWeeklyAssetSettings(List<AssetSettingsGetDBResponse> request, Guid userUid, Guid customerUID)
		{
			try
			{
				_loggingService.Debug("Started Publishing Asset Weekly Settings", "AssetSettingsPublisher.PublishUserWeeklyAssetSettings");
				var userWeeklyAssetTargetEvents = new List<KafkaMessage>();
				foreach (var req in request)
				{
					_loggingService.Debug(string.Format("Current UserAssetWeeklyRequest : {0}", JsonConvert.SerializeObject(req)), "AssetSettingsPublisher.PublishUserWeeklyAssetSettings");
					var userWeeklyAssetTargetEvent = new UserAssetWeeklyTargetEvent
					{
						AssetUID = req.AssetUID,
						EndDate = req.EndDate,
						SundayTargetValue = req.Sunday,
						MondayTargetValue = req.Monday,
						TuesdayTargetValue = req.Tuesday,
						WednesdayTargetValue = req.Wednesday,
						ThursdayTargetValue = req.Thursday,
						FridayTargetValue = req.Friday,
						SaturdayTargetValue = req.Saturday,
						StartDate = req.StartDate,
						TargetType = (AssetTargetType)req.ConfigValue,
						Timestamp = new TimestampDetail { EventUtc = DateTime.UtcNow },
						UserUID = userUid,
						CustomerUID = customerUID
					};
					var payloadMessage = new KafkaMessage { Key = userWeeklyAssetTargetEvent.AssetUID.ToString(), Message = userWeeklyAssetTargetEvent, Topic = _userAssetWeeklySettingsTopic }; // check we need to publish with hypens or not
					_loggingService.Debug(string.Format("Message To Be Published to Kafka :- {0} ", JsonConvert.SerializeObject(payloadMessage)), "AssetSettingsPublisher.PublishUserWeeklyAssetSettings");
					_loggingService.Debug(string.Format("USer Asset Weekly Settings Published to Kafka :- {0} ", JsonConvert.SerializeObject(payloadMessage)), "AssetSettingsPublisher.PublishUserWeeklyAssetSettings");
					userWeeklyAssetTargetEvents.Add(payloadMessage);
				}
				_transactions.Publish(userWeeklyAssetTargetEvents);
			}
			catch (Exception ex)
			{
				this._loggingService.Error("An Exception has occurred ", MethodInfo.GetCurrentMethod().Name, ex);
			}
		}

		public bool PublishAssetSettings(IEnumerable<AssetSettingsDto> settingsDto)
		{
			List<KafkaMessage> payloadMessages = new List<KafkaMessage>();
			try
			{
				_loggingService.Debug("Started Publishing Asset Settings", "AssetSettingsPublisher.PublishAssetSettings");
				foreach (var assetSetting in settingsDto)
				{
					var assetTargetEvent = new AssetTargetEvent
					{
						AssetTargetUID = assetSetting.AssetConfigUID,
						AssetUID = assetSetting.AssetUID,
						TargetType = (AssetTargetType)Enum.Parse(typeof(AssetTargetType), assetSetting.TargetType, true),
						StartDate = Convert.ToDateTime(assetSetting.StartDate),
						TargetValue = assetSetting.TargetValue,
						InsertUTC = Convert.ToDateTime(assetSetting.InsertUTC),
						UpdateUTC = Convert.ToDateTime(assetSetting.UpdateUTC),
						StatusInd = assetSetting.StatusInd
					};
					if (assetSetting.EndDate.HasValue)
					{
						assetTargetEvent.EndDate = Convert.ToDateTime(assetSetting.EndDate);
					}
					var payloadMessage = new KafkaMessage { Key = assetTargetEvent.AssetUID.ToString(), Message = assetTargetEvent, Topic = _assetSettingsTopic }; // check we need to publish with hypens or not
					payloadMessages.Add(payloadMessage);
					_loggingService.Debug(string.Format("Following Daily Asset Settings will be published to Kafka :- {0}", JsonConvert.SerializeObject(payloadMessage)), "AssetSettingsPublisher.PublishAssetSettings");
				}
				_loggingService.Debug("Started Publishing Asset Setting messages with count : " + payloadMessages.Count, "AssetSettingsPublisher.PublishAssetSettings");

				_transactions.Publish(payloadMessages);
				_loggingService.Debug("Ended Publishing Asset Settings", "AssetSettingsPublisher.PublishAssetSettings");
				return true;
			}
			catch (Exception ex)
			{
				this._loggingService.Error("An Exception has occurred", "AssetSettingsPublisher.PublishAssetSettings", ex);
				return false;
			}
		}
	}
}
