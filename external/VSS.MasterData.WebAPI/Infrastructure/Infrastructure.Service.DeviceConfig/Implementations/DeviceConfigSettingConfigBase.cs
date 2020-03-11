using CommonModel.DeviceSettings;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.DeviceTypeGroupParameterAttribute;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Logging;
using Infrastructure.Common.DeviceMessageConstructor.Attributes;
using Infrastructure.Common.DeviceMessageConstructor.Implementation;
using Infrastructure.Common.DeviceMessageConstructor.Interfaces;

namespace Infrastructure.Service.DeviceConfig.Implementations
{
	public class DeviceConfigSettingConfigBase : IDeviceConfigSettingConfig
    {
        private readonly ILoggingService _loggingService;
        private readonly IDictionary<string, IGroupSettingsConfig> _groupContainer = new Dictionary<string, IGroupSettingsConfig>();
        private readonly IDataPopulator _dataPopulator;

        public DeviceConfigSettingConfigBase(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _loggingService.CreateLogger(this.GetType());
            _dataPopulator = new DataPopulator();
            _groupContainer = GetGroupBuilderTypes();
        }

        private Dictionary<string, IGroupSettingsConfig> GetGroupBuilderTypes()
        {
            var groupBuilderTypes = typeof(IGroupSettingsConfig);
            return groupBuilderTypes.Assembly.GetTypes().Where(groupBuilderTypes.IsAssignableFrom).Where(x => x.IsClass && x.IsPublic).ToDictionary(GetGroupName, y => Activator.CreateInstance(y, _loggingService, _dataPopulator) as IGroupSettingsConfig);
        }

        private string GetGroupName(Type type)
        {
            var groupObj = type.GetCustomAttributes(typeof(GroupAttribute), false).FirstOrDefault() as GroupAttribute;
            return groupObj != null ? groupObj.GroupName : type.Name;
        }

        public DeviceConfigurationSettingsConfig GetSettingsConfig(DeviceConfigRequestBase requestBase, IDictionary<string, DeviceTypeGroupParameterAttributeDetails> attributeIds)
        {
            this._loggingService.Info("Device Request Message : " + JsonConvert.SerializeObject(requestBase), "DeviceConfigSettingConfigBase.GetSettingsConfig");
            if (requestBase == null || !_groupContainer.ContainsKey(requestBase.ParameterGroupName))
            {
                this._loggingService.Info("Device Message Group Name / Device Type is invalid !!", "DeviceConfigSettingConfigBase.GetSettingsConfig");
                return null;
            }
            var groupBuilder = _groupContainer[requestBase.ParameterGroupName];
            var groupSettings = groupBuilder.GetSettingsConfig(attributeIds);
            return groupSettings;
        }
    }
}
