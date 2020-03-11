using System.Collections.Generic;
using System.Threading.Tasks;
using CommonModel.Error;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.Switches;

namespace Infrastructure.Service.DeviceConfig.Interfaces
{
    public interface ISwitchesValidator : IRequestValidator<DeviceConfigSwitchesRequest>
    {
        Task<IList<IErrorInfo>> ValidateDualStateSwitches(DeviceConfigSwitchesRequest request);
    }
}
