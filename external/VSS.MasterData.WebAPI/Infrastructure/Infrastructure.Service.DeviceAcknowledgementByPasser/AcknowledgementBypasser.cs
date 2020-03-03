using ClientModel.DeviceConfig.Request.DeviceConfig;
using CommonModel.DeviceSettings;
using Infrastructure.Common.DeviceMessageConstructor.Attributes;
using Infrastructure.Common.DeviceMessageConstructor.Implementation;
using Infrastructure.Common.DeviceMessageConstructor.Interfaces;
using Infrastructure.Service.DeviceAcknowledgementByPasser.Interfaces;
using Infrastructure.Service.DeviceMessagePublisher;
using Infrastructure.Service.DeviceMessagePublisher.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;
using VSS.VisionLink.Interfaces.Events.DeviceConfig;
using ParamGroup = CommonModel.DeviceSettings.ParamGroup;

namespace Infrastructure.Service.DeviceAcknowledgementByPasser
{
	public class AcknowledgementBypasser : IAckBypasser
	{
		private static ILoggingService _loggingService;
		private readonly IDictionary<string, IGroupMessageBuilder> _groupContainer = new Dictionary<string, IGroupMessageBuilder>();
		private readonly IDataPopulator _dataPopulator;
		private readonly IDeviceMessageKafkaPublisher _kafkaPublisher;

		public AcknowledgementBypasser(IOptions<Configurations> configuration, ITransactions transaction)
		{
			_dataPopulator = new DataPopulator();
			_kafkaPublisher = new DeviceMessageKafkaPublisher(configuration.Value, transaction);
			_groupContainer = GetGroupBuilderTypes();
		}

		private Dictionary<string, IGroupMessageBuilder> GetGroupBuilderTypes()
		{
			var groupBuilderTypes = typeof(IGroupMessageBuilder);
			return groupBuilderTypes.Assembly.GetTypes().Where(groupBuilderTypes.IsAssignableFrom).Where(x => x.IsClass && x.IsPublic).ToDictionary(GetGroupName, y => Activator.CreateInstance(y, _dataPopulator) as IGroupMessageBuilder);
		}

		private string GetGroupName(Type type)
		{
			var groupObj = type.GetCustomAttributes(typeof(GroupAttribute), false).FirstOrDefault() as GroupAttribute;
			return groupObj != null ? groupObj.GroupName : type.Name;
		}

		public bool ProcessBypassMessage(DeviceConfigRequestBase requestBase, ParamGroup group)
		{
			_loggingService.Debug("Device Request Message : " + JsonConvert.SerializeObject(requestBase), "");
			var failedAssets = String.Empty;

			try
			{
				if (requestBase == null || !_groupContainer.ContainsKey(requestBase.ParameterGroupName))
				{
					_loggingService.Info("Device Message Group Name / Device Type is invalid !!", "");
					return false;
				}
				var groupBuilder = _groupContainer[requestBase.ParameterGroupName];
				foreach (var assetuid in requestBase.AssetUIDs)
				{
					_loggingService.Info("Started processing group messages - AcknowledgementBypasser.ProcessBypassMessage", "");
					var groupMessages = groupBuilder.ProcessGroupMessages(assetuid, requestBase, group);
					var assetId = new Guid(assetuid);
					_loggingService.Info("Started publishing messages - AcknowledgementBypasser.ProcessBypassMessage", "");
					if (!_kafkaPublisher.PublishMessage(assetId.ToString(), groupMessages, ""))
					{
						failedAssets += (failedAssets == String.Empty ? "" : ",") + assetuid;
					}
				}
				if (failedAssets == String.Empty)
					return true;
			}
			catch (Exception ex)
			{
				_loggingService.Error("Exception : " + JsonConvert.SerializeObject(ex), "", ex);
				throw new Exception("Unable to Complete Device Configuration for assetuid " + failedAssets);
			}
			return false;
		}

		public bool PublishConfiguredMessage(DeviceConfigMessage deviceConfigMessage, IEnumerable<Guid> deviceUIDs)
		{
			_loggingService.Info("Started publishing configured messages - AcknowledgementBypasser.PublishConfiguredMessage", "");
			bool result = false;
			var deviceConfiguredMsgs = new List<DeviceConfig>();
			try
			{
				foreach (var parameter in deviceConfigMessage.Group.Parameters)
				{
					foreach (var deviceUID in deviceUIDs)
					{
						var deviceConfiguredMsg = new VSS.VisionLink.Interfaces.Events.DeviceConfig.DeviceConfig
						{
							DeviceUID = deviceUID,
							Group = new VSS.VisionLink.Interfaces.Events.DeviceConfig.ParamGroup
							{
								GroupName = deviceConfigMessage.Group.GroupName,
								Parameters = new List<VSS.VisionLink.Interfaces.Events.DeviceConfig.Parameter>()
							},
							Timestamp = new VSS.VisionLink.Interfaces.Events.DeviceConfig.Context.TimestampDetail
							{
								EventUtc = deviceConfigMessage.Timestamp.EventUtc
							}
						};
						var param = new VSS.VisionLink.Interfaces.Events.DeviceConfig.Parameter();
						param.Attributes = new List<VSS.VisionLink.Interfaces.Events.DeviceConfig.Attributes>();
						foreach (var attribute in parameter.Attributes)
						{
							var attr = new VSS.VisionLink.Interfaces.Events.DeviceConfig.Attributes
							{
								AttributeName = attribute.AttributeName,
								AttributeValue = attribute.AttributeValue
							};
							param.Attributes.Add(attr);
						}
						param.ParameterName = parameter.ParameterName;
						deviceConfiguredMsg.Group.Parameters.Add(param);
						deviceConfiguredMsgs.Add(deviceConfiguredMsg);
					}
				}
				result = _kafkaPublisher.PublishDeviceConfiguredMessage(deviceConfiguredMsgs);
			}
			catch (Exception ex)
			{
				_loggingService.Error("An error has occured : " + JsonConvert.SerializeObject(ex), "", ex);
			}
			return result;
		}
	}

}
