using ClientModel.DeviceConfig.Request.DeviceConfig;
using CommonModel.DeviceSettings;
using System;
using System.Collections.Generic;

namespace Infrastructure.Service.DeviceAcknowledgementByPasser.Interfaces
{
	public interface IAckBypasser
    {
        bool ProcessBypassMessage(DeviceConfigRequestBase requestBase, ParamGroup group);
        bool PublishConfiguredMessage(DeviceConfigMessage deviceConfiguredMsg, IEnumerable<Guid> deviceUID);
    }
}
