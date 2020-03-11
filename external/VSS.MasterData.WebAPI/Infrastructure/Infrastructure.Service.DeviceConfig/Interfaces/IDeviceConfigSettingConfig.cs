using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonModel.DeviceSettings;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.DeviceTypeGroupParameterAttribute;

namespace Infrastructure.Service.DeviceConfig.Interfaces
{
    public interface IDeviceConfigSettingConfig
    {
        DeviceConfigurationSettingsConfig GetSettingsConfig(DeviceConfigRequestBase requestBase, IDictionary<string, DeviceTypeGroupParameterAttributeDetails> attributeIds);
    }
}
