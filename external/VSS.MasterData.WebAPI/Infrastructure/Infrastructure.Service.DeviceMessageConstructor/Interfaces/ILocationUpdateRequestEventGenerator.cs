using CommonModel.DeviceSettings;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;

namespace Infrastructure.Service.DeviceMessageConstructor.Interfaces
{
	public interface ILocationUpdateRequestEventGenerator
    {
        IOutMessageEvent GetLocationMessageForA5N2(DeviceDetails deviceDetails);
        IMTSOutMessageEvent GetLocationMessageForMTS(DeviceDetails deviceDetails);
        IPLOutMessageEvent GetLocationMessageForPLDevices(DeviceDetails deviceDetails);

    }
}
