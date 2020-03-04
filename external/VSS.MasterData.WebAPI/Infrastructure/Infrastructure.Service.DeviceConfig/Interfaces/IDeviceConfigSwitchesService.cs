using System.Threading.Tasks;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.Switches;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.Switches;

namespace Infrastructure.Service.DeviceConfig.Interfaces
{
    public interface IDeviceConfigSwitchesService
    {
        Task<DeviceConfigServiceResponse<DeviceConfigConfiguredDualStateSwitchInfo>> GetConfiguredDualStateSwitches(DeviceConfigSwitchesRequest request);
    }
}
