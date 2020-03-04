using ClientModel.DeviceConfig.Response.DeviceConfig.DeviceTypeGroupParameterAttribute;
using CommonModel.DeviceSettings;
using Infrastructure.Common.DeviceMessageConstructor.Attributes;
using Infrastructure.Common.DeviceMessageConstructor.Interfaces;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Utilities.Logging;

namespace Infrastructure.Service.DeviceConfig.Implementations
{
	[Group("Meters")]
    public class MetersSettingsConfig : IGroupSettingsConfig
    {
        private readonly ILoggingService _loggingService;
        private readonly IDataPopulator _dataPopulator;

        public MetersSettingsConfig(ILoggingService loggingService, IDataPopulator dataPopulator)
        {
            _dataPopulator = dataPopulator;
            _loggingService = loggingService;
            _loggingService.CreateLogger(this.GetType());
        }

        public DeviceConfigurationSettingsConfig GetSettingsConfig(IDictionary<string, DeviceTypeGroupParameterAttributeDetails> attributeIds)
        {
            var hoursMeter = attributeIds.FirstOrDefault(x => x.Value.AttributeName == "HoursMeter");
            var deviceConfigurationValues = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(hoursMeter.Value.DefaultValueJSON);

            if (deviceConfigurationValues != null && deviceConfigurationValues.Configurations != null)
            {
                if (deviceConfigurationValues.Configurations.SettingsConfig != null)
                {
                    return deviceConfigurationValues.Configurations.SettingsConfig;
                }
            }
            return null;
        }
    }
}
