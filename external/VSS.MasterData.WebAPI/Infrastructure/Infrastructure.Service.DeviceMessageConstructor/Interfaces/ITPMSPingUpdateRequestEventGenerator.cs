using CommonModel.DeviceSettings;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;

namespace Infrastructure.Service.DeviceMessageConstructor.Interfaces
{
    public interface ITPMSPingUpdateRequestEventGenerator
    {
        IMTSOutMessageEvent GetTPMSRequestMessageForMTS(DeviceDetails deviceDetails);
    }
}
