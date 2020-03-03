using CommonModel.DeviceSettings;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;

namespace Infrastructure.Service.DeviceMessageConstructor.Interfaces
{
    public interface IEventDiagonsticUpdateRequestEventGenerator
    {
        IPLOutMessageEvent GetEventDiagonsticUpdateRequestEventGenerator(DeviceDetails deviceDetails);
    }
}
