using ClientModel.DeviceConfig.Request.DeviceConfig;
using CommonModel.DeviceSettings;
using System.Collections.Generic;

namespace Infrastructure.Service.DeviceAcknowledgementByPasser.Interfaces
{
	public interface IGroupMessageBuilder
    {
        IEnumerable<object> ProcessGroupMessages(string assetuid, DeviceConfigRequestBase requestBase, ParamGroup group);
    }
}
