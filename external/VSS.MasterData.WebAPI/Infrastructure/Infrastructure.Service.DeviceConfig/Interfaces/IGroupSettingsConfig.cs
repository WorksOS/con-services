using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonModel.DeviceSettings;
using ClientModel.DeviceConfig.Response.DeviceConfig.DeviceTypeGroupParameterAttribute;

namespace Infrastructure.Service.DeviceConfig.Interfaces
{
    public interface IGroupSettingsConfig
    {
        DeviceConfigurationSettingsConfig GetSettingsConfig(IDictionary<string, DeviceTypeGroupParameterAttributeDetails> attributeIds);
    }
}
