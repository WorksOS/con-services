using ClientModel.DeviceConfig.Request.DeviceConfig;
using DbModel.DeviceConfig;
using CommonModels = CommonModel.DeviceSettings;
using DeviceConfigRepository;
using Infrastructure.Common.DeviceMessageConstructor.Attributes;
using Infrastructure.Common.DeviceMessageConstructor.Exceptions;
using Infrastructure.Common.DeviceMessageConstructor.Implementation;
using Infrastructure.Common.DeviceMessageConstructor.Interfaces;
using CommonModel.DeviceSettings;
using Infrastructure.Common.DeviceMessageConstructor.Validator;
using Infrastructure.Service.DeviceMessageConstructor.Implementations;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Infrastructure.Service.DeviceMessagePublisher;
using Infrastructure.Service.DeviceMessagePublisher.Interfaces;
using Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace Infrastructure.Service.DeviceMessageConstructor
{
	public class MessageConstructor : IMessageConstructor
    {
        private readonly ILoggingService _loggingService;
        private readonly IMessageConstructorDeviceService _deviceService;
        private readonly IDictionary<string, DeviceTypeFamily> _deviceTypeFamilyContainer = new Dictionary<string, DeviceTypeFamily>();
        private readonly IDictionary<string, IGroupMessageEventBuilder> _groupContainer = new Dictionary<string, IGroupMessageEventBuilder>();        
        private readonly IDataPopulator _dataPopulator;
        private readonly IDataValidator _dataValidator;
        private readonly IDeviceMessageKafkaPublisher _kafkaPublisher;
        private readonly IUpdateDeviceRequestStatusBuilder _updateRequestStatusEventBuilder;
        private readonly IUpdateDeviceRequestMessageBuilder _updateEventMessageBuilder;

		public MessageConstructor(IOptions<CommonModels.Configurations> configurations, ITransactions transactions, ILoggingService loggingService)
        {
			_loggingService = loggingService;
			_deviceService = new MessageConstructorDeviceService(configurations);
            _dataPopulator = new DataPopulator();
            _kafkaPublisher = new DeviceMessageKafkaPublisher(configurations.Value, transactions);
            _dataValidator = new DataValidator();

            _updateRequestStatusEventBuilder = new UpdateDeviceRequestStatusBuilder(new LocationUpdateRequestEventGenerator(), new FuelUpdateRequestEventGenerator(), new ECMInfoUpdateRequestEventGenerator(),
                                                                                    new TPMSPingUpdateRequestEventGenerator(), new PTOHoursVia1939UpdateRequestEventGenerator(), new BatteryVoltageVia1939UpdateRequestEventGenerator(), 
                                                                                    new EventDiagonsticUpdateRequestEventGenerator());
            _updateEventMessageBuilder = new UpdateDeviceRequestMessageBuilder(_updateRequestStatusEventBuilder);
            _deviceTypeFamilyContainer = _deviceService.GetDeviceTypeFamily();
            _groupContainer = GetGroupBuilderTypes();
		}

        private Dictionary<string, IGroupMessageEventBuilder> GetGroupBuilderTypes()
        {
            var groupBuilderTypes = typeof(IGroupMessageEventBuilder);
            return groupBuilderTypes.Assembly.GetTypes().Where(groupBuilderTypes.IsAssignableFrom).Where(x => x.IsClass && x.IsPublic).ToDictionary(GetGroupName, y => Activator.CreateInstance(y, _dataPopulator, _dataValidator, _loggingService) as IGroupMessageEventBuilder);
        }

        private string GetGroupName(Type type)
        {
            var groupObj = type.GetCustomAttributes(typeof(GroupAttribute), false).FirstOrDefault() as GroupAttribute;
            return groupObj != null ? groupObj.GroupName : type.Name;
        }

        public Tuple<bool, List<DeviceConfigMsg>> ProcessMessage(DeviceConfigRequestBase requestBase, CommonModels.DeviceConfigMessage configMessage)
        {
			var failedAssets = String.Empty;
			try
			{
				_loggingService.Debug("Device Request Message : " + JsonConvert.SerializeObject(requestBase), "MessageConstructor.ProcessMessage");
				_loggingService.Debug("Device Config Message : " + JsonConvert.SerializeObject(configMessage), "MessageConstructor.ProcessMessage");
				if (requestBase == null || configMessage == null || !_groupContainer.ContainsKey(configMessage.Group.GroupName) || !_deviceTypeFamilyContainer.ContainsKey(requestBase.DeviceType))
				{
					_loggingService.Info("Device Message Group Name / Device Type is invalid !!", "MessageConstructor.ProcessMessage");
					return Tuple.Create<bool, List<DeviceConfigMsg>>(false,null);
				}
				var deviceDetails = new List<DeviceDetails>();
				var kafkaObjects = new List<object>();
				var groupBuilder = _groupContainer[configMessage.Group.GroupName];
				var deviceTypeFamily = _deviceTypeFamilyContainer[requestBase.DeviceType];
				var failedAssetsList = new List<string>();
				var deviceDataDetails = this.GetDeviceData(requestBase.AssetUIDs, requestBase, configMessage);
				switch (deviceTypeFamily.FamilyName)
				{
					case "DataOut":
						failedAssetsList.AddRange(this.ProcessDataOutMessage(groupBuilder, requestBase, deviceDetails, kafkaObjects, configMessage, deviceDataDetails));
						break;
					case "MTS":
						failedAssetsList.AddRange(this.ProcessMtsOutMessage(groupBuilder, requestBase, deviceDetails, kafkaObjects, configMessage, deviceDataDetails));
						break;
					case "PL":
						failedAssetsList.AddRange(this.ProcessPLOutMessage(groupBuilder, requestBase, deviceDetails, kafkaObjects, configMessage, deviceDataDetails));
						break;
				}
				var deviceConfigMsgs = GetDeviceConfigMsg(deviceDetails).ToList();
				if (!(failedAssets.Any() || !PersistPublish(deviceConfigMsgs, kafkaObjects)))
				{
					return Tuple.Create(true, deviceConfigMsgs);
				}
				else
				{
					Exception ex = new MessageConstructorException("Unable to Complete Device Configuration for assetuid " + failedAssets);
					_loggingService.Error("Unable to Complete Device Configuration for assetuid " + failedAssets, "MessageConstructor.ProcessMessage", ex);
					throw ex;
				}
			}
			catch (Exception ex)
			{
				_loggingService.Error("Unable to Complete Device Configuration for assetuid " + failedAssets, "MessageConstructor.ProcessMessage", ex);
				throw new MessageConstructorException("Unable to Complete Device Configuration for assetuid " + failedAssets);
			}
		}

		private List<string> ProcessDataOutMessage(IGroupMessageEventBuilder groupBuilder, DeviceConfigRequestBase requestBase, List<DeviceDetails> deviceDetails, List<object> kafkaObjects, CommonModels.DeviceConfigMessage configMessage, IEnumerable<DeviceDetails> deviceDataDetails)
		{
			var failedAssets = new List<string>();
			var dataOutEvents = new List<object>();
			var dataOutDeviceDetails = new List<DeviceDetails>();
			foreach (var assetuid in requestBase.AssetUIDs)
			{
				var deviceData = deviceDataDetails.First(x => x.AssetUid == Guid.Parse(assetuid));
				_loggingService.Info("Processing Message Started : " + JsonConvert.SerializeObject(deviceData), "MessageConstructor.ProcessDataOutMessage");
				var dataOutObj = groupBuilder.GetDataOutMessageEvent(requestBase, deviceData);
				if (!_dataValidator.TryParseEventMessage(dataOutObj, ref dataOutEvents, ref dataOutDeviceDetails))
					failedAssets.Add(assetuid);
			}
			kafkaObjects.AddRange(dataOutEvents);
			deviceDetails.AddRange(dataOutDeviceDetails);
			return failedAssets;
		}

		private List<string> ProcessMtsOutMessage(IGroupMessageEventBuilder groupBuilder, DeviceConfigRequestBase requestBase, List<DeviceDetails> deviceDetails, List<object> kafkaObjects, CommonModels.DeviceConfigMessage configMessage, IEnumerable<DeviceDetails> deviceDataDetails)
		{
			var failedAssets = new List<string>();
			var mtsOutEvents = new List<object>();
			var mtsOutDeviceDetails = new List<DeviceDetails>();
			foreach (var assetuid in requestBase.AssetUIDs)
			{
				var deviceData = deviceDataDetails.First(x => x.AssetUid == Guid.Parse(assetuid));
				_loggingService.Info("Processing Message Started : " + JsonConvert.SerializeObject(deviceData), "MessageConstructor.ProcessMtsOutMessage");
				var dataOutObj = groupBuilder.GetMtsOutMessageEvent(requestBase, deviceData);
				if (!_dataValidator.TryParseEventMessage(dataOutObj, ref mtsOutEvents, ref mtsOutDeviceDetails))
					failedAssets.Add(assetuid);
			}
			kafkaObjects.AddRange(mtsOutEvents);
			deviceDetails.AddRange(mtsOutDeviceDetails);
			return failedAssets;
		}

		private List<string> ProcessPLOutMessage(IGroupMessageEventBuilder groupBuilder, DeviceConfigRequestBase requestBase, List<DeviceDetails> deviceDetails, List<object> kafkaObjects, CommonModels.DeviceConfigMessage configMessage, IEnumerable<DeviceDetails> deviceDataDetails)
		{
			var failedAssets = new List<string>();
			var plOutEvents = new List<object>();
			var plOutDeviceDetails = new List<DeviceDetails>();
			foreach (var assetuid in requestBase.AssetUIDs)
			{
				var deviceData = deviceDataDetails.First(x => x.AssetUid == Guid.Parse(assetuid));
				_loggingService.Info("Processing Message Started : " + JsonConvert.SerializeObject(deviceData), "MessageConstructor.ProcessPLOutMessage");
				var dataOutObj = groupBuilder.GetPlOutMessageEvent(requestBase, deviceData);
				if (!_dataValidator.TryParseEventMessage(dataOutObj, ref plOutEvents, ref plOutDeviceDetails))
					failedAssets.Add(assetuid);
			}
			kafkaObjects.AddRange(plOutEvents);
			deviceDetails.AddRange(plOutDeviceDetails);
			return failedAssets;
		}
        private IEnumerable<DeviceDetails> GetDeviceData(List<string> assetUids, DeviceConfigRequestBase requestBase, CommonModels.DeviceConfigMessage configMessage)
        {
            var deviceDatas = _deviceService.GetDeviceData(assetUids, requestBase.DeviceType);

			if (deviceDatas.Any() && assetUids.Count() > deviceDatas.Count())
            {
				var noAssetDeviceList = assetUids.Except(deviceDatas.Select(x => x.AssetUid));
				if (noAssetDeviceList.Any())
				{
					_loggingService.Info("Unable to find the device for the asset : " + string.Join(",",noAssetDeviceList), "MessageConstructor.GetDeviceData");
					throw new MessageConstructorException("Unable to find the device for the asset : " + string.Join(",", noAssetDeviceList));
				}
			}
            var deviceDetails = deviceDatas.Select(x => new DeviceDetails
			{
				AssetUid = new Guid(x.AssetUid),
				DeviceUid = new Guid(x.DeviceUid),
				EventUtc = configMessage.Timestamp.EventUtc,
				DeviceType = requestBase.DeviceType,
				Group = configMessage.Group,
				SerialNumber = x.SerialNumber,
				UserUid = requestBase.UserUID.HasValue ? requestBase.UserUID.Value : default(Guid)
			});

            return deviceDetails;
        }

		private bool PersistPublish(IEnumerable<DeviceConfigMsg> deviceDetails, IEnumerable<object> kafkaObjects)
		{
            try
			{
				var deviceDetailsCount = deviceDetails.Count();
				if (deviceDetailsCount > 0 && deviceDetailsCount == kafkaObjects.Count())
				{ 
					if (_kafkaPublisher.PublishMessage(kafkaObjects, _deviceTypeFamilyContainer[deviceDetails.First().DeviceType].FamilyName))
					{
						return true;
					}
					throw new MessageConstructorException("Failed to publish message to Kafka");					
				}
			}
			catch(Exception ex)
			{
				_loggingService.Error("Error in MessageConstructor.PersistPublish ", "MessageConstructor.PersistPublish", ex);
				throw ex;
			}
			return false;
		}

        private bool PersistPublish(IEnumerable<DeviceACKMessage> deviceDetails, IEnumerable<object> kafkaObjects, string deviceTypeFamilyName)
        {
            var deviceDetailsCount = deviceDetails.Count();
            if (deviceDetailsCount > 0 && deviceDetailsCount == kafkaObjects.Count())
            {
                var deviceDetail = deviceDetails.First();
                using (var transactionScope = new TransactionScope())
                {
                    if (_deviceService.PersistDeviceACKMessage(deviceDetails))
                    {
                        if (_kafkaPublisher.PublishMessage(deviceDetail.DeviceUID.ToString(), kafkaObjects, deviceTypeFamilyName))
                        {
                            transactionScope.Complete();
							_loggingService.Info("Device Message Event Published Successfully for " + deviceDetail.DeviceUID + " and " + deviceDetail.RowUpdatedUTC, "MessageConstructor.PersistPublish");
                            return true;
                        }
                        throw new MessageConstructorException("Failed to publish message to Kafka");
                    }
                }
            }
            return false;
        }

        private IEnumerable<DeviceConfigMsg> GetDeviceConfigMsg(IEnumerable<DeviceDetails> deviceDetails)
        {
            var deviceConfigMsgs = new List<DeviceConfigMsg>();

			deviceDetails.ToList().ForEach(e => deviceConfigMsgs.Add(
                new DeviceConfigMsg
                {
                    AssetUid = e.AssetUid,
                    DeviceUid = e.DeviceUid,
                    EventUtc = e.EventUtc,
                    DeviceType = e.DeviceType,
                    MessageContent = Deserialize(new { Group = e.Group }),
                    MessageUid = e.MessageUid,
                    SerialNumber = e.SerialNumber,
                    UserUid = e.UserUid
                }));
            return deviceConfigMsgs;
        }

        public Guid ProcessMessage(Guid assetUID, Guid deviceUID, string deviceTypeFamily)
        {
            _loggingService.Debug("Ping Message Constructor - Started", "MessageConstructor.ProcessMessage");
            _loggingService.Debug(string.Format("AssetUID : {0}, DeviceUID : {1}, DeviceTypeFamily : {2}", assetUID, deviceUID, deviceTypeFamily), "MessageConstructor.ProcessMessage");
            if (assetUID == Guid.Empty || deviceUID == Guid.Empty || string.IsNullOrEmpty(deviceTypeFamily))
            {
                _loggingService.Info("Device Message Group Name / Device Type is invalid !!", "MessageConstructor.ProcessMessage");
                return Guid.Empty;
            }
            _loggingService.Debug("Before Calling Capabilities Data", "MessageConstructor.ProcessMessage");
            var deviceData = _deviceService.GetDeviceSupportedFeatures(assetUID.ToString(), deviceUID.ToString());
            _loggingService.Debug("After Calling Capabilities Data", "MessageConstructor.ProcessMessage");
            if (deviceData == null || !deviceData.Any())
            {
                _loggingService.Info("No Supported Features For This Device", "MessageConstructor.ProcessMessage");
                return Guid.Empty;
            }
            _loggingService.Debug("Before Calling Device Data", "MessageConstructor.ProcessMessage");
            var deviceDetails = _deviceService.GetDeviceData(assetUID.ToString());
            _loggingService.Debug("After Calling Device Data", "MessageConstructor.ProcessMessage");
            var deviceInfo = new DeviceDetails
            {
                AssetUid = assetUID,
                DeviceUid = deviceUID,
                SerialNumber = deviceDetails.SerialNumber,
                EventUtc = DateTime.UtcNow,
                DeviceType = deviceDetails.DeviceType,
            };
            IList<DeviceACKMessage> deviceAckMessage = new List<DeviceACKMessage>();
            var _kafkaObjects = new List<object>();
            var devicePing_loggingServiceUID = _updateEventMessageBuilder.GetUpdateRequestForDeviceType(deviceInfo, deviceTypeFamily, deviceData, ref deviceAckMessage, ref _kafkaObjects);

            if (PersistPublish(deviceAckMessage, _kafkaObjects, deviceTypeFamily))
                return devicePing_loggingServiceUID;

            throw new Exception("Message Construction Failed");
        }

		private string Deserialize(object value)
		{
			var serializer = new JsonSerializer();
			var stringWriter = new StringWriter();
			using (var writer = new JsonTextWriter(stringWriter))
			{
				writer.QuoteName = false;
				serializer.Serialize(writer, value);
			}
			return stringWriter.ToString();
		}
    }
}
