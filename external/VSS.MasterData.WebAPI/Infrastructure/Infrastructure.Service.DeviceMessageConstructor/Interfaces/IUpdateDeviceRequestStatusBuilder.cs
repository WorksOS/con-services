using CommonModel.DeviceSettings;
using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;

namespace Infrastructure.Service.DeviceMessageConstructor.Interfaces
{
	public interface IUpdateDeviceRequestStatusBuilder
    {
        IEnumerable<IPLOutMessageEvent> BuildPLDeviceStatusUpdateRequestMessage(DeviceDetails deviceDetails, IDictionary<string, string> _deviceCapability);
        IEnumerable<IMTSOutMessageEvent> BuildMTSDeviceStatusUpdateRequestMessage(DeviceDetails deviceDetails, IDictionary<string, string> _deviceCapability);
        IEnumerable<IOutMessageEvent> BuildA5N2DeviceStatusUpdateRequestMessage(DeviceDetails deviceDetails, IDictionary<string, string> _deviceCapability);
    }
}
