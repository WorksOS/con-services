using CommonModel.DeviceSettings;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;

namespace Infrastructure.Service.DeviceMessageConstructor.Interfaces
{
    public interface IBatteryVoltageVia1939UpdateRequestEventGenerator
    {
        IMTSOutMessageEvent GetBatteryVoltageVia1939(DeviceDetails deviceDetails);
    }
}
