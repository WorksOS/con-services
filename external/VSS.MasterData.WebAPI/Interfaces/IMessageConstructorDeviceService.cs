using DbModel.DeviceConfig;
using System.Collections.Generic;

namespace Interfaces
{
	public interface IMessageConstructorDeviceService
    {
        IDictionary<string, DeviceTypeFamily> GetDeviceTypeFamily();
        bool PersistDeviceConfig(IEnumerable<DeviceConfigMsg> deviceConfigMessage);
        DeviceData GetDeviceData(string assetUid);
        IDictionary<string, string> GetDeviceSupportedFeatures(string assetUID, string deviceUID);
        IEnumerable<DeviceData> GetDeviceData(List<string> assetUids, string deviceType);
        bool PersistDeviceACKMessage(IEnumerable<DeviceACKMessage> deviceACKMessage);
    }
}
