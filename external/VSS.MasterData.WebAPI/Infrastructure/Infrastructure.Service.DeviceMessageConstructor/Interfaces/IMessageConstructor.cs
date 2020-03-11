using ClientModel.DeviceConfig.Request.DeviceConfig;
using CommonModel.DeviceSettings;
using DbModel.DeviceConfig;
using System;
using System.Collections.Generic;

namespace Infrastructure.Service.DeviceMessageConstructor.Interfaces
{
	public interface IMessageConstructor
   {
        Tuple<bool, List<DeviceConfigMsg>> ProcessMessage(DeviceConfigRequestBase requestBase, DeviceConfigMessage configMessage);
        Guid ProcessMessage(Guid assetUID, Guid deviceUID, string deviceTypeFamily);
    }
}
