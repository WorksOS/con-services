using AutoMapper;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.DeviceTypeGroupParameterAttribute;
using CommonModel.DeviceSettings;
using CommonModel.DeviceSettings.ConfigNameValues;
using DbModel.DeviceConfig;
using DeviceConfigRepository;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceAcknowledgementByPasser.Interfaces;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Utilities.IOC;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.Utilities.Extensions;

namespace Infrastructure.Service.DeviceConfig.Implementations
{
	public abstract class DeviceConfigRepositoryServiceBase : DeviceConfigServiceBase
	{
		private readonly IDeviceConfigRepository _deviceConfigRepository;
		protected readonly ConfigNameValueCollection _attributeMaps;
		protected readonly IMessageConstructor _messageConstructor;
		protected readonly IAckBypasser _ackBypasser;
		protected readonly IDeviceConfigSettingConfig _settingsConfig;
		private readonly IMessageConstructorDeviceService _deviceService;
		protected readonly IDictionary<string, DeviceTypeFamily> _deviceTypeFamilyContainer = new Dictionary<string, DeviceTypeFamily>();
		private readonly IAssetDeviceRepository _assetDeviceRepository;
		//private readonly string _assetDeviceConfigApiUrl;
		private readonly List<string> _pendingClearOffDeviceTypes;
		private readonly int _pendingClearOffDays;
		private readonly IOptions<Configurations> _configurations;
		private readonly ITransactions _transactions;

		protected DeviceConfigRepositoryServiceBase(IInjectConfig injectInfo, IDeviceConfigRepository deviceConfigRepository, IParameterAttributeCache parameterAttributeCache, IMapper mapper,
			IAssetDeviceRepository assetDeviceRepository, 
			IMessageConstructor messageConstructor, 
			IAckBypasser ackBypasser, 
			IDeviceConfigSettingConfig settingsConfig,
			IOptions<Configurations> configurations,
			ITransactions transactions,
			ILoggingService loggingService) : base(parameterAttributeCache, mapper, loggingService)
		{
			this._attributeMaps = injectInfo.ResolveKeyed<DeviceConfigAttributeToRequestMaps>("DeviceConfigAttributeToRequestMaps");
			this._deviceConfigRepository = deviceConfigRepository;
			this._loggingService.CreateLogger(this.GetType());
			this._messageConstructor = messageConstructor;
			this._ackBypasser = ackBypasser;
			this._settingsConfig = settingsConfig;
			this._assetDeviceRepository = assetDeviceRepository;
			this._deviceService = new MessageConstructorDeviceService(configurations);
			this._deviceTypeFamilyContainer = _deviceService.GetDeviceTypeFamily();
			this._configurations = configurations;
			//this._assetDeviceConfigApiUrl = ConfigurationManager.AppSettings["PendingDeviceConfigApiUri"];
			this._pendingClearOffDeviceTypes = _configurations.Value.AppSettings.ClearOffPendingValueForDeviceTypes.Split(',').ToList();
			this._pendingClearOffDays = _configurations.Value.AppSettings.ClearOffPendingValueGreaterThanNoOfDays;
			this._transactions = transactions;
		}

		protected async virtual Task<List<DeviceConfigDto>> Fetch(DeviceConfigRequestBase request)
		{
			this._loggingService.Info("Fetching for DeviceConfigRequestBase with request : " + JsonConvert.SerializeObject(request), "DeviceConfigRepositoryServiceBase.Fetch");

			IList<DeviceConfigDto> deviceConfigDtos = new List<DeviceConfigDto>();

			await this.GetAttributeIds(request.DeviceType, request.ParameterGroupName);			

			foreach (var attributeId in base._paramterAttributeDetails)
			{
				deviceConfigDtos.Add(new DeviceConfigDto
				{
					DeviceTypeParameterID = attributeId.Value.DeviceTypeParameterID,
					DeviceParameterAttributeId = attributeId.Value.DeviceParamAttrID
				});
			}

			this._loggingService.Info("Started Invoking DeviceConfigRepository with request : " + JsonConvert.SerializeObject(request), "DeviceConfigRepositoryServiceBase.Fetch");

			var deviceConfigResponseDtos = await this._deviceConfigRepository.Fetch(request.AssetUIDs, deviceConfigDtos);

			this._loggingService.Info("Ended Invoking DeviceConfigRepository with response : " + JsonConvert.SerializeObject(deviceConfigResponseDtos), "DeviceConfigRepositoryServiceBase.Fetch");

			return this.AssignPendingInfo(request, deviceConfigResponseDtos);
		}

		private List<DeviceConfigDto> AssignPendingInfo(DeviceConfigRequestBase request, IEnumerable<DeviceConfigDto> deviceConfigDtos)
		{
			/*
			 * (CASE
				WHEN(DC.LastAttrEventUTC IS NULL OR DC.FutureAttrEventUTC >= DC.LastAttrEventUTC) THEN DC.FutureAttributeValue
				ELSE DC.AttributeValue
			END) AS AttributeValue,
			(CASE
				WHEN(DC.FutureAttrEventUTC IS NOT NULL AND DC.FutureAttrEventUTC >= DC.LastAttrEventUTC) THEN DC.FutureAttrEventUTC
				WHEN(DC.LastAttrEventUTC IS NOT NULL AND DC.LastAttrEventUTC >= DC.FutureAttrEventUTC) THEN DC.LastAttrEventUTC
				ELSE DC.UpdateUTC
			END) AS UpdateUTC,
			(CASE 
				WHEN (DC.LastAttrEventUTC IS NOT NULL AND DC.LastAttrEventUTC >= DC.FutureAttrEventUTC) THEN 0
				ELSE 1
			END) AS IsPending
			*/

			foreach(var deviceConfigDto in deviceConfigDtos)
			{
				deviceConfigDto.AttributeValue = deviceConfigDto.FutureAttributeValue;
				deviceConfigDto.IsPending = true;
				deviceConfigDto.UpdateUTC = deviceConfigDto.RowUpdatedUTC;

				if (deviceConfigDto.FutureAttributeEventUTC.HasValue)
				{
					if (_pendingClearOffDeviceTypes.Contains(request.DeviceType) && DateTime.UtcNow.Subtract(deviceConfigDto.FutureAttributeEventUTC.Value).Days > _pendingClearOffDays)
					{
						deviceConfigDto.IsPending = false;
						deviceConfigDto.AttributeValue = deviceConfigDto.CurrentAttributeValue;
						deviceConfigDto.UpdateUTC = deviceConfigDto.LastAttributeEventUTC.HasValue ? deviceConfigDto.LastAttrEventUTC : deviceConfigDto.RowUpdatedUTC;
					}
					else
					{
						if (deviceConfigDto.LastAttributeEventUTC.HasValue)
						{
							if (deviceConfigDto.LastAttributeEventUTC.Value >= deviceConfigDto.FutureAttributeEventUTC.Value)
							{
								deviceConfigDto.AttributeValue = deviceConfigDto.CurrentAttributeValue;
								deviceConfigDto.UpdateUTC = deviceConfigDto.LastAttrEventUTC;
								deviceConfigDto.IsPending = false;
							}
							else
							{
								deviceConfigDto.UpdateUTC = deviceConfigDto.FutureAttrEventUTC;
							}
						}
					}
				}
				else
				{
					deviceConfigDto.AttributeValue = deviceConfigDto.CurrentAttributeValue;
					deviceConfigDto.UpdateUTC = deviceConfigDto.LastAttributeEventUTC.HasValue ? deviceConfigDto.LastAttrEventUTC : deviceConfigDto.UpdateUTC;
					deviceConfigDto.IsPending = false;
				}
			}
			return deviceConfigDtos.ToList();
		}

		private async Task UpdateCurrentValue(DeviceConfigDto deviceConfigDto)
		{
			try
			{
				this._loggingService.Info("UpdateCurrentValue in Device Config : " + JsonConvert.SerializeObject(deviceConfigDto), "DeviceConfigRepositoryServiceBase.UpdateCurrentValue");
				await this._deviceConfigRepository.UpdateCurrentValue(deviceConfigDto);
			}
			catch (Exception ex)
			{
				this._loggingService.Error("An Exception has occurred", "DeviceConfigRepositoryServiceBase.UpdateDeviceConfig", ex);
			}
		}


		protected async virtual Task<IList<DeviceConfigDto>> Save(DeviceConfigRequestBase request)
		{
			try
			{
				await this.GetAttributeIds(request.DeviceType, request.ParameterGroupName);

				var deviceConfigResponseDtos = await this.Fetch(request);

				var availableAssetUIDs = deviceConfigResponseDtos.Select(x => Guid.Parse(x.AssetUIDString)).Distinct().ToList();

				var assetDeviceDtos = await this._assetDeviceRepository.FetchByAssetUIDAndDeviceType(request.AssetUIDs, request.DeviceType);

				var assetDeviceMaps = assetDeviceDtos.ToDictionary(x => x.AssetUID, x => x.DeviceUID);

				this._loggingService.Info("Already available assets : " + JsonConvert.SerializeObject(availableAssetUIDs), "DeviceConfigRepositoryServiceBase.Save");

				var assetsTobeInserted = request.AssetUIDs.Select(x => Guid.Parse(x)).Except(availableAssetUIDs).ToList();

				this._loggingService.Info("New assets : " + JsonConvert.SerializeObject(assetsTobeInserted), "DeviceConfigRepositoryServiceBase.Save");

				var currentDateTimeUTC = DateTime.UtcNow.ToDateTimeStringWithYearMonthDayFormat();

				//for new assets to be inserted
				foreach (var asset in assetsTobeInserted)
				{
					foreach (var attributeId in base._paramterAttributeDetails)
					{
						if (request.ConfigValues.ContainsKey(attributeId.Key))
						{
							var configValue = request.ConfigValues[GetAttributeName(attributeId)];
							if (assetDeviceMaps.ContainsKey(asset))
							{
								deviceConfigResponseDtos.Add(new DeviceConfigDto
								{
									DeviceUIDString = assetDeviceMaps[asset].ToString("N"),
									AssetUIDString = asset.ToString("N"),
									DeviceTypeParameterID = attributeId.Value.DeviceTypeParameterID,
									DeviceParameterAttributeId = attributeId.Value.DeviceParamAttrID,
									FutureAttrEventUTC = currentDateTimeUTC,
									RowUpdatedUTC = currentDateTimeUTC,
									RowInsertedUTC = currentDateTimeUTC,
									AttributeName = attributeId.Value.AttributeName,
									ParameterName = attributeId.Value.ParameterName
								});
							}
						}
					}
				}

				var newAttributes = new List<DeviceConfigDto>();

				//for left out attribute which is not in DB
				foreach (var availableAsset in availableAssetUIDs)
				{
					foreach (var attributeId in base._paramterAttributeDetails)
					{
						this._loggingService.Info("Already available assets : " + JsonConvert.SerializeObject(availableAssetUIDs), "DeviceConfigRepositoryServiceBase.Save");
						if (assetDeviceMaps.ContainsKey(availableAsset))
						{ 
							if (!isAttributeNameExists(deviceConfigResponseDtos, attributeId, assetDeviceMaps[availableAsset].ToString("N")) && request.ConfigValues.ContainsKey(attributeId.Key))
							{
								var configValue = request.ConfigValues[GetAttributeName(attributeId)];
								newAttributes.Add(new DeviceConfigDto
								{
									DeviceUIDString = assetDeviceMaps[availableAsset].ToString("N"),
									AssetUIDString = availableAsset.ToString("N"),
									DeviceTypeParameterID = attributeId.Value.DeviceTypeParameterID,
									DeviceParameterAttributeId = attributeId.Value.DeviceParamAttrID,
									FutureAttrEventUTC = currentDateTimeUTC,
									RowUpdatedUTC = currentDateTimeUTC,
									RowInsertedUTC = currentDateTimeUTC,
									AttributeName = attributeId.Value.AttributeName,
									ParameterName = attributeId.Value.ParameterName
								});
							}
						}
					}
				}

				if (newAttributes.Any())
				{
					deviceConfigResponseDtos.AddRange(newAttributes);
				}

				//assign the value and update the Event UTC
				foreach (var config in deviceConfigResponseDtos)
				{
					if (request.ConfigValues.ContainsKey(GetConfigAttributeName(config)))
					{
						config.FutureAttributeValue = request.ConfigValues[GetConfigAttributeName(config)];
						config.RowUpdatedUTC = currentDateTimeUTC;
						config.FutureAttrEventUTC = currentDateTimeUTC;
					}
				}

				//remove device config dtos which are not configured now(so no updates or insert will be done)
				deviceConfigResponseDtos.RemoveAll(x => !request.ConfigValues.ContainsKey(GetAttributeNameForConfig(x)));

				await this.ProcessAndSaveDeviceConfig(request, deviceConfigResponseDtos, currentDateTimeUTC, assetDeviceMaps);

				var response = await this.Fetch(request);

				return response;
			}
			catch (Exception ex)
			{
				this._loggingService.Error("An Error has occurred", "DeviceConfigRepositoryServiceBase.Save", ex);
				throw ex;
			}
		}

		private DeviceConfigMessage BuildDeviceConfigMessage(DeviceConfigRequestBase request, string currentDateTimeUTC)
		{
			var deviceConfigMessage = new DeviceConfigMessage
			{
				Group = new ParamGroup
				{
					GroupName = request.ParameterGroupName,
					Parameters = new List<Parameter>()
				},
				Timestamp = new TimestampDetail
				{
					EventUtc = DateTime.Parse(currentDateTimeUTC)
				}
			};

			foreach (var item in request.ConfigValues)
			{
				if (this._paramterAttributeDetails.ContainsKey(item.Key))
				{
					var parameterAttribute = item.Key.Split('.');
					var parameter = deviceConfigMessage.Group.Parameters.FirstOrDefault(x => x.ParameterName == parameterAttribute[0]);
					if (parameter == null)
					{
						parameter = new Parameter
						{
							ParameterName = parameterAttribute[0]
						};
					}
					if (parameter.Attributes == null)
					{
						parameter.Attributes = new List<AttributeDetails>();
					}

					parameter.Attributes.Add(
						new AttributeDetails
						{
							AttributeName = parameterAttribute[1],
							AttributeValue = item.Value
						});

					if (!deviceConfigMessage.Group.Parameters.Any(x => x.ParameterName == parameterAttribute[0]))
					{
						deviceConfigMessage.Group.Parameters.Add(parameter);
					}
				}
			}
			return deviceConfigMessage;
		}

		private async Task ProcessAndSaveDeviceConfig(DeviceConfigRequestBase request, List<DeviceConfigDto> deviceConfigResponseDtos, string currentDateTimeUTC, Dictionary<Guid,Guid> assetDeviceMaps)
		{
			this._loggingService.Info("Started Invoking ProcessAndSaveDeviceConfig", "DeviceConfigRepositoryServiceBase.BuildResponse");

			var deviceConfigMsgs = new List<DeviceConfigMsg>();

			var deviceConfigMessage = this.BuildDeviceConfigMessage(request, currentDateTimeUTC);

			// To check for following a different path for TAP66/76 devices as these device types will not acknowledge the device config values
			var settingConfig = _settingsConfig.GetSettingsConfig(request, base._paramterAttributeDetails);

			this._loggingService.Info("Invoking Message Constructor with DeviceConfigRequest : " + JsonConvert.SerializeObject(deviceConfigMessage) + " and DeviceConfigMessage : " + JsonConvert.SerializeObject(deviceConfigMessage), "DeviceConfigRepositoryServiceBase.BuildResponse");
			var deviceConfigProcessedMsg = await ProcessMessage(settingConfig, request, deviceConfigMessage, deviceConfigMsgs);

			if (deviceConfigProcessedMsg.Item1)
			{
				var deviceUIDs = assetDeviceMaps.Values;

				SaveDeviceConfigAndDeviceConfigMessage(deviceConfigResponseDtos, deviceConfigProcessedMsg.Item2);

				//Update Current Value for TAP66/76 as the ackowledgement processor will not do this
				await UpdateCurrentAttributeValues(settingConfig, request);

				var pendingDeviceConfigMessage = this.BuildPendingDeviceConfigRequest(request, deviceUIDs, DateTime.Parse(currentDateTimeUTC));

				//this._loggingService.Info("Invoking Pending Device Config api with url : " + _assetDeviceConfigApiUrl + " with request : " + JsonConvert.SerializeObject(pendingDeviceConfigMessage), "DeviceConfigRepositoryServiceBase.BuildResponse");

				//await _apiClient.PutAsync<object>(_assetDeviceConfigApiUrl, pendingDeviceConfigMessage);

				//this._loggingService.Info("Ended Invoking Device Config api", "DeviceConfigRepositoryServiceBase.BuildResponse");

				//Construct DeviceConfiguredMessage to publish for TAP66/76 deviceTypes as the devices will not acknowledge the parameters. Hence we are by passing Acknowledgement processor by publishing from API itself
				if (settingConfig != null && settingConfig.AllowAdditionalTopic)
				{
					var isConfigured = _ackBypasser.PublishConfiguredMessage(deviceConfigMessage, deviceUIDs);
				}

			}
			this._loggingService.Info("Ended Invoking SaveDeviceConfig", "DeviceConfigRepositoryServiceBase.BuildResponse");
		}

		private async Task UpdateCurrentAttributeValues(DeviceConfigurationSettingsConfig settingsConfig, DeviceConfigRequestBase request)
		{
			if (settingsConfig != null && !settingsConfig.SendToDevice)
			{
				//Dont insert into DeviceConfigMessage table. Just process the msg and publish to new Kafka topics
				this._loggingService.Info("Started Invoking UpdateCurrentAttributeValues", "DeviceConfigRepositoryServiceBase.UpdateCurrentAttributeValues");
				var updateDeviceConfigResponseDtos = await this.Fetch(request);
				UpdateCurrentValues(updateDeviceConfigResponseDtos.ToList());
				this._loggingService.Info("Ended Invoking UpdateCurrentAttributeValues", "DeviceConfigRepositoryServiceBase.UpdateCurrentAttributeValues");
			}
		}

		private async Task<Tuple<bool, List<DeviceConfigMsg>>> ProcessMessage(DeviceConfigurationSettingsConfig settingsConfig, DeviceConfigRequestBase request, DeviceConfigMessage deviceConfigMessage, List<DeviceConfigMsg> deviceConfigMsgs)
		{
			if (settingsConfig != null && !settingsConfig.SendToDevice)
			{
				//Dont insert into DeviceConfigMessage table. Just process the msg and publish to new Kafka topics
				this._loggingService.Info("Started Invoking ProcessBypassMessage", "DeviceConfigRepositoryServiceBase.ProcessMessage");
				return Tuple.Create<bool, List<DeviceConfigMsg>>(_ackBypasser.ProcessBypassMessage(request, deviceConfigMessage.Group), null);
			}
			else
			{
				this._loggingService.Info("Started Invoking ProcessMessage", "DeviceConfigRepositoryServiceBase.ProcessMessage");
				return _messageConstructor.ProcessMessage(await GetRequestAndHandleForNullCases(request), deviceConfigMessage);
			}
		}

		private PendingDeviceConfigRequest BuildPendingDeviceConfigRequest(DeviceConfigRequestBase deviceConfigMessage, IEnumerable<Guid> deviceUIDs, DateTime currentDateTimeUTC)
		{
			IList<AssetDeviceConfigRequestDto> assetDeviceConfigRequestDtos = new List<AssetDeviceConfigRequestDto>();

			this._loggingService.Info("Started building pending device config with request : " + JsonConvert.SerializeObject(deviceConfigMessage), "DeviceConfigRepositoryServiceBase.BuildPendingDeviceConfigRequest");

			foreach (var config in deviceConfigMessage.ConfigValues)
			{
				var keys = config.Key.Split('.');

				if (_paramterAttributeDetails.ContainsKey(config.Key))
				{
					assetDeviceConfigRequestDtos.Add(new AssetDeviceConfigRequestDto
					{
						DeviceType = deviceConfigMessage.DeviceType,
						GroupName = deviceConfigMessage.ParameterGroupName,
						ParameterName = keys[0],
						AttributeName = keys[1],
						AttributeValue = config.Value,
						ActionUTC = currentDateTimeUTC,
						DeviceUIDs = deviceUIDs
					});
				}
			}

			this._loggingService.Info("Ended building pending device config", "DeviceConfigRepositoryServiceBase.BuildPendingDeviceConfigRequest");

			return new PendingDeviceConfigRequest
			{
				PendingDeviceConfigs = assetDeviceConfigRequestDtos
			};
		}

		protected virtual bool isAttributeNameExists(IList<DeviceConfigDto> deviceConfigResponseDtos, KeyValuePair<string, DeviceTypeGroupParameterAttributeDetails> attributeId, string deviceUID)
		{
			return deviceConfigResponseDtos.Where(x => string.Compare(x.DeviceUIDString, deviceUID) == 0).Any(x => x.AttributeName == attributeId.Value.AttributeName);
		}

		protected async virtual Task<DeviceConfigRequestBase> GetRequestAndHandleForNullCases(DeviceConfigRequestBase request)
		{
			return request;
		}

		private void SaveDeviceConfigAndDeviceConfigMessage(List<DeviceConfigDto> revisedAssetGroupedDeviceConfigDtos, List<DeviceConfigMsg> deviceDetails)
		{
			List<Action> transactStatements = new List<Action>();
			if (revisedAssetGroupedDeviceConfigDtos.Any())
			{
				//using (var dbConnection = _databaseManager.GetConnection())
				
					if (deviceDetails != null && deviceDetails.Any())
					{
						this._loggingService.Info("Started updating Device Config Message table for insertion / updation", "DeviceConfigRepositoryServiceBase.Save");
						//var deviceConfigMessageQuery = _transactions.GetUpsertBuilder<DeviceConfigMessageDto>();
						var deviceConfigMessages = (deviceDetails.Select(x => new DeviceConfigMessageDto
						{
							DeviceConfigMessageUID = x.MessageUid,
							DeviceUID = x.DeviceUid,
							UserUID = x.UserUid,
							DeviceTypeID = _deviceTypeFamilyContainer[x.DeviceType].DeviceTypeId, //Rephrase
							EventUTCString = x.EventUtc.ToDateTimeStringWithYearMonthDayFormat(),
							MessageContent = x.MessageContent,
							StatusID = 0,
							LastMessageUTCString = DateTime.UtcNow.ToDateTimeStringWithYearMonthDayFormat()
						}));
						transactStatements.Add(() => _transactions.Upsert(deviceConfigMessages));
						this._loggingService.Info("Ended updating Device Config Message table for insertion / updation", "DeviceConfigRepositoryServiceBase.Save");
					}

					if (revisedAssetGroupedDeviceConfigDtos != null && revisedAssetGroupedDeviceConfigDtos.Any())
					{
						this._loggingService.Info("Started updating Device Config table for insertion / updation", "DeviceConfigRepositoryServiceBase.Save");
					//var upsertQuery = _databaseManager.GetUpsertBuilder<DeviceConfigDto>();
					//upsertQuery.AddRows(revisedAssetGroupedDeviceConfigDtos);
					transactStatements.Add(() => _transactions.Upsert<DeviceConfigDto>(revisedAssetGroupedDeviceConfigDtos));
						this._loggingService.Info("Ended updating Device Config table for insertion / updation", "DeviceConfigRepositoryServiceBase.Save");
					}
					
					if ((deviceDetails != null && deviceDetails.Any()) || (revisedAssetGroupedDeviceConfigDtos != null && revisedAssetGroupedDeviceConfigDtos.Any()))
					{
						_transactions.Execute(transactStatements);
						//dbConnection.Commit(); //TODO: need to check if its really needed
					}
				
			}
		}

		private void UpdateCurrentValues(List<DeviceConfigDto> deviceConfigDtos)
		{
			this._loggingService.Info("Started parallel execution for updation of current attribute values", "DeviceConfigRepositoryServiceBase.UpdateCurrentValues");

			Parallel.ForEach(deviceConfigDtos, async deviceConfigDto =>
			{
				deviceConfigDto.RowUpdatedUTC = DateTime.UtcNow.ToDateTimeStringWithYearMonthDayFormat();
				await this.UpdateCurrentValue(deviceConfigDto);
			});

			this._loggingService.Info("Ended parallel execution for updation of current attribute values", "DeviceConfigRepositoryServiceBase.UpdateCurrentValues");
		}
		protected virtual string GetAttributeNameForConfig(DeviceConfigDto x)
		{
			return x.ParameterName + "." + x.AttributeName;
		}

		protected virtual string GetConfigAttributeName(DeviceConfigDto config)
		{
			return config.ParameterName + "." + config.AttributeName;
		}

		protected virtual string GetAttributeName(KeyValuePair<string, DeviceTypeGroupParameterAttributeDetails> attributeId)
		{
			return attributeId.Key;
		}


		protected virtual IList<TOut> BuildResponse<TIn, TOut>(TIn request, IList<DeviceConfigDto> deviceConfigDtos) where TIn : DeviceConfigRequestBase
																											 where TOut : DeviceConfigResponseBase, new()
		{
			var deviceConfigServiceResponseDetails = new List<TOut>();

			var cachedParameterAttributes = this._parameterAttributeCache.Get(request.DeviceType, request.ParameterGroupName).Result;

			this._loggingService.Info("Building Response objects from DeviceConfigDtos", "DeviceConfigRepositoryServiceBase.BuildResponse");

			foreach (var deviceConfigs in deviceConfigDtos.GroupBy(x => x.AssetUIDString))
			{
				this._loggingService.Info("Building Response object for AssetUID : " + deviceConfigs.First().AssetUIDString + " and Group : " + request.ParameterGroupName + " with the Response model : " + typeof(TOut).Name, "DeviceConfigRepositoryServiceBase.BuildResponse");

				dynamic result = new TOut();
				DeviceConfigDto configDto = null;
				foreach (var deviceConfig in deviceConfigs)
				{
					var propertyName = this._attributeMaps.Values[deviceConfig.ParameterName + "." + deviceConfig.AttributeName];

					if (!string.IsNullOrEmpty(propertyName))
					{
						this._loggingService.Info("Assigning property value for name : " + propertyName + " and value : " + deviceConfig.AttributeValue, "DeviceConfigRepositoryServiceBase.BuildResponse");

						TimeSpan timeSpan;
						DateTime dateTime;
						if (!string.IsNullOrEmpty(deviceConfig.AttributeValue))
						{
							if (deviceConfig.AttributeValue.Contains(":"))
							{
								if (TimeSpan.TryParse(deviceConfig.AttributeValue, out timeSpan))
								{
									SetValue(result, propertyName, timeSpan);
								}
								else if (DateTime.TryParse(deviceConfig.AttributeValue, out dateTime))
								{
									SetValue(result, propertyName, dateTime);
								}
							}
							else
							{
								SetValue(result, propertyName, deviceConfig.AttributeValue);
							}
						}
					}
					else
					{
						this._loggingService.Error("No property mapping available : " + deviceConfig.ParameterName + "." + deviceConfig.AttributeName + " and value : " + deviceConfig.AttributeValue, "DeviceConfigRepositoryServiceBase.BuildResponse", new Exception("No property mapping available : " + deviceConfig.ParameterName + "." + deviceConfig.AttributeName + " and value : " + deviceConfig.AttributeValue));
					}
					if (configDto == null)
					{
						configDto = deviceConfig;
					}
				}
				if (configDto != null)
				{
					result.AssetUID = Guid.Parse(configDto.AssetUIDString);
					result.LastUpdatedOn = Convert.ToDateTime(configDto.UpdateUTC);
				}
				deviceConfigServiceResponseDetails.Add(result);
			}
			return deviceConfigServiceResponseDetails;
		}

		protected void SetValue(object inputObject, string propertyName, object propertyVal)
		{
			//find out the type
			var type = inputObject.GetType();
			try
			{
				//get the property information based on the type
				var propertyInfo = type.GetProperty(propertyName);
				if (propertyInfo != null)
				{
					//find the property type
					var propertyType = propertyInfo.PropertyType;

					//Convert.ChangeType does not handle conversion to nullable types
					//if the property type is nullable, we need to get the underlying type of the property
					var targetType = IsNullableType(propertyInfo.PropertyType) ? Nullable.GetUnderlyingType(propertyInfo.PropertyType) : propertyInfo.PropertyType;

					//Returns an System.Object with the specified System.Type and whose value is
					//equivalent to the specified object.
					propertyVal = Convert.ChangeType(propertyVal, targetType);

					//Set the value of the property
					propertyInfo.SetValue(inputObject, propertyVal, null);
				}
			}
			catch (Exception ex)
			{
				this._loggingService.Error("Error occurred while assigning value : " + propertyVal.ToString() + " to property name : " + propertyName, "DeviceConfigRepositoryServiceBase.SetValue", ex);
			}
		}
		private static bool IsNullableType(Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
		}

		protected object GetValue<TIn>(TIn details, string attributeName)
		{
			return typeof(TIn).GetProperty(this._attributeMaps.Values[attributeName]).GetValue(details, null);
		}
	}
}
