using CommonModel.DeviceSettings;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;

namespace Infrastructure.Service.DeviceMessageConstructor.Interfaces
{
    public interface IECMInfoUpdateRequestEventGenerator
    {
        IMTSOutMessageEvent GetECMRequestMessageForMTS(DeviceDetails deviceDetails);
        IMTSOutMessageEvent GetDevicePersonalityRequest(DeviceDetails deviceDetails);
    }
}
