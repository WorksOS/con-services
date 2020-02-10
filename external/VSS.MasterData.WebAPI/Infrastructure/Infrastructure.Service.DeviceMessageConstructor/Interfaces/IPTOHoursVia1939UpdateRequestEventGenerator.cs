using CommonModel.DeviceSettings;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;

namespace Infrastructure.Service.DeviceMessageConstructor.Interfaces
{
    public interface IPTOHoursVia1939UpdateRequestEventGenerator
    {
        IMTSOutMessageEvent GetPTOHoursviaJ1939(DeviceDetails deviceDetails);
    }
}
